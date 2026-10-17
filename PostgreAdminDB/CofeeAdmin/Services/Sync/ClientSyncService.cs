using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CofeeAdmin.Services.Sync
{
    public class ClientSyncService : IClientSyncService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ClientSyncService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task NotifyTableRefreshAsync()
        {
            var configuredUrl = _configuration["ClientApp:BaseUrl"] ?? "https://localhost:7185";
            var candidateUrls = new[]
            {
                configuredUrl.TrimEnd('/'),
                "http://localhost:5203",
                "https://localhost:7185"
            };

            foreach (var baseUrl in candidateUrls)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("InsecureClient");
                    client.Timeout = TimeSpan.FromSeconds(3);
                    var response = await client.GetAsync($"{baseUrl}/Reservation/TriggerTableRefresh");
                    if (response.IsSuccessStatusCode)
                    {
                        // Başarıyla tetiklendi
                        return;
                    }
                }
                catch
                {
                    // Bir sonraki portu dene
                }
            }
        }
    }
}
