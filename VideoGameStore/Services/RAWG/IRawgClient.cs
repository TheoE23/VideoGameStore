using VideoGameStore.Models.Rawg;

namespace VideoGameStore.Services.Rawg
{
    public interface IRawgClient
    {
        Task<List<RawgGame>> GetTopGamesAsync(int count);
        Task<RawgGameDetails> GetGameDetailsAsync(int rawgGameId);
    }
}
