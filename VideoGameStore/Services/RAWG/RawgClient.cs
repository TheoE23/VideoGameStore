using System.Net.Http.Json;
using VideoGameStore.Models.Rawg;

namespace VideoGameStore.Services.Rawg
{
    public class RawgClient : IRawgClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RawgClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Rawg:ApiKey"]!;
        }

        public async Task<List<RawgGame>> GetTopGamesAsync(int count)
        {
            var url = $"games?ordering=-ratings_count&page_size={count}&key={_apiKey}";

            var response = await _httpClient
                .GetFromJsonAsync<RawgGameListResponse>(url);

            return response?.Results ?? new List<RawgGame>();
        }

        public async Task<RawgGameDetails> GetGameDetailsAsync(int rawgGameId)
        {
            var url = $"games/{rawgGameId}?key={_apiKey}";

            return await _httpClient
                .GetFromJsonAsync<RawgGameDetails>(url)
                   ?? throw new Exception("RAWG game not found");
        }
    }
}
