using System.Threading.Tasks;

namespace CofeeAdmin.Services.Sync
{
    public interface IClientSyncService
    {
        Task NotifyTableRefreshAsync();
    }
}
