using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.RestaurantTable;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RestaurantTableController : Controller
    {
        private readonly IRestaurantTableService _restaurantTableService;
        private readonly IMapper _mapper;
        private readonly CofeeAdmin.Services.Sync.IClientSyncService? _clientSyncService;

        public RestaurantTableController(IRestaurantTableService restaurantTableService, IMapper mapper, CofeeAdmin.Services.Sync.IClientSyncService? clientSyncService = null)
        {
            _restaurantTableService = restaurantTableService;
            _mapper = mapper;
            _clientSyncService = clientSyncService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tables = await _restaurantTableService.GetListAsync(
                predicate: table => table.DeletedAt == null,
                orderBy: query => query.OrderBy(table => table.TableNumber));

            var layout = tables.Select(table => new SaveRestaurantTableLayoutDto
            {
                TableNumber = table.TableNumber,
                PositionX = table.PositionX,
                PositionY = table.PositionY,
                Rotation = table.Rotation,
                Shape = table.Shape,
                Capacity = table.Capacity
            }).ToList();

            return View(layout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(List<AddRestaurantTableDto> addRestaurantTableDtos)
        {
            if (addRestaurantTableDtos == null || addRestaurantTableDtos.Count == 0)
            {
                TempData["TableLayoutError"] = "Kaydedilecek masa bulunamadı.";
                return RedirectToAction("Index");
            }

            if (addRestaurantTableDtos.GroupBy(table => table.TableNumber).Any(group => group.Count() > 1))
            {
                TempData["TableLayoutError"] = "Gönderilen listede aynı masa numarası birden fazla kullanılmış.";
                return RedirectToAction("Index");
            }

            var requestedNumbers = addRestaurantTableDtos.Select(table => table.TableNumber).ToArray();

            var existingTables = await _restaurantTableService.GetListAsync(
                predicate: table => requestedNumbers.Contains(table.TableNumber));

            var existingNumbers = existingTables.Select(table => table.TableNumber).ToHashSet();

            var newTableDtos = addRestaurantTableDtos
                .Where(table => !existingNumbers.Contains(table.TableNumber))
                .ToList();

            if (newTableDtos.Count == 0)
            {
                TempData["TableLayoutError"] = "Bu masa numaraları zaten kayıtlı. Mevcut masaları değiştirmek için güncelleme işlemi gerekir.";
                return RedirectToAction(nameof(Index));
            }

            var entities = _mapper.Map<List<RestaurantTable>>(newTableDtos);

            await _restaurantTableService.AddRangeAsync(entities);

            var skippedCount = addRestaurantTableDtos.Count - entities.Count;

            TempData["TableLayoutSaved"] = skippedCount == 0
                ? $"{entities.Count} yeni masa veritabanına eklendi."
                : $"{entities.Count} yeni masa eklendi; masa numarası zaten kayıtlı olan {skippedCount} masa atlandı.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLayout(List<SaveRestaurantTableLayoutDto> layout)
        {
            if (layout == null || layout.Count == 0)
            {
                TempData["TableLayoutError"] = "Kaydedilecek masa bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (layout.Any(table => table.TableNumber < 1
                || table.PositionX < 0 || table.PositionX > 1
                || table.PositionY < 0 || table.PositionY > 1
                || table.Rotation < 0 || table.Rotation >= 360
                || table.Capacity < 1
                || (table.Shape != "round" && table.Shape != "square")))
            {
                TempData["TableLayoutError"] = "Masa bilgilerinden biri geçersiz. Lütfen masa numarası, konum, şekil ve kapasiteyi kontrol edin.";
                return RedirectToAction(nameof(Index));
            }

            if (layout.GroupBy(table => table.TableNumber).Any(group => group.Count() > 1))
            {
                TempData["TableLayoutError"] = "Yerleşimde aynı masa numarası birden fazla kullanılmış.";
                return RedirectToAction(nameof(Index));
            }

            var tableNumbers = layout.Select(table => table.TableNumber).ToHashSet();
            var existingTables = await _restaurantTableService.GetListAsync();
            var activeByNumber = existingTables
                .Where(table => table.DeletedAt == null)
                .ToDictionary(table => table.TableNumber);
            var deletedByNumber = existingTables
                .Where(table => table.DeletedAt != null)
                .GroupBy(table => table.TableNumber)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(table => table.UpdateAt).First());

            var newTables = new List<RestaurantTable>();

            var changedTables = new List<RestaurantTable>();

            var unchangedCount = 0;

            foreach (var item in layout)
            {
                if (!activeByNumber.TryGetValue(item.TableNumber, out var table))
                {
                    if (deletedByNumber.TryGetValue(item.TableNumber, out table))
                    {
                        _mapper.Map(item, table);
                        table.DeletedAt = null;
                        table.UpdateAt = DateTime.UtcNow;
                        changedTables.Add(table);
                        deletedByNumber.Remove(item.TableNumber);
                        continue;
                    }

                    newTables.Add(_mapper.Map<RestaurantTable>(item));
                    continue;
                }

                var hasChanges = Math.Abs(table.PositionX - item.PositionX) > 0.000001
                    || Math.Abs(table.PositionY - item.PositionY) > 0.000001
                    || Math.Abs(table.Rotation - item.Rotation) > 0.000001
                    || table.Shape != item.Shape
                    || table.Capacity != item.Capacity;

                if (!hasChanges)
                {
                    unchangedCount++;
                    continue;
                }

                _mapper.Map(item, table);
                table.UpdateAt = DateTime.UtcNow;
                changedTables.Add(table);
            }

            var deletedCount = 0;
            var now = DateTime.UtcNow;
            foreach (var table in activeByNumber.Values.Where(table => !tableNumbers.Contains(table.TableNumber)))
            {
                table.DeletedAt = now;
                table.UpdateAt = now;
                changedTables.Add(table);
                deletedCount++;
            }

            await _restaurantTableService.AddOrUpdateRangeAsync(newTables, changedTables);

            if (_clientSyncService != null)
            {
                await _clientSyncService.NotifyTableRefreshAsync();
            }

            TempData["TableLayoutSaved"] = $"Yerleşim kaydedildi: {newTables.Count} yeni masa eklendi, {changedTables.Count - deletedCount} masa güncellendi, {deletedCount} masa silindi, {unchangedCount} masa değişmedi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
