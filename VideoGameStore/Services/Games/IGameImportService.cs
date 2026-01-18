namespace VideoGameStore.Services.Games
{
    public interface IGameImportService
    {
        Task ImportTopGamesAsync(int count);
    }
}
