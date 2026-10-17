using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using X.PagedList;
using Domain.Packages;

namespace Application.Features.Reservation.Queries.GetMyReservatioLs
{
    public class GetMyRservationListQuery : IRequest<IPagedList<GetMyRservationListDto>>
    {
        public PagedResult pagedResult;
    }
    public class GetMyRservationListHandler : IRequestHandler<GetMyRservationListQuery, IPagedList<GetMyRservationListDto>>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public GetMyRservationListHandler(IReservationRepository reservationRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<IPagedList<GetMyRservationListDto>> Handle(GetMyRservationListQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("Rezervasyonlarınızı görüntülemek için giriş yapmalısınız.");

            if (!int.TryParse(userId, out var currentUserId))
                throw new UnauthorizedAccessException("Oturum kullanıcı bilgisi geçersiz. Lütfen yeniden giriş yapın.");

            var list = await _reservationRepository.GetListAsync(predicate: n => n.UserId == currentUserId, page: request.pagedResult.page, pageSize: request.pagedResult.pageSize, include: n => n.Include(p => p.User));

            var items = _mapper.Map<List<GetMyRservationListDto>>(list);
            return new StaticPagedList<GetMyRservationListDto>(
                items,
                list.PageNumber,
                list.PageSize,
                list.TotalItemCount);
        }
    }
}
