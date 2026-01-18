using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;
using VideoGameStore.Services.Rawg;

namespace VideoGameStore.Services.Games
{
    public class GameImportService : IGameImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRawgClient _rawgClient;

        public GameImportService(ApplicationDbContext context, IRawgClient rawgClient)
        {
            _context = context;
            _rawgClient = rawgClient;
        }

        public async Task ImportTopGamesAsync(int count)
        {
            var rawgGames = await _rawgClient.GetTopGamesAsync(count);

            foreach (var rawgGame in rawgGames)
            {
                if (await _context.Games.AnyAsync(g => g.RawgGameId == rawgGame.Id))
                    continue;

                var details = await _rawgClient.GetGameDetailsAsync(rawgGame.Id);

                Developer? developer = null;
                var rawgDev = details.Developers.FirstOrDefault();

                if (rawgDev != null)
                {
                    developer = await _context.Developers
                        .FirstOrDefaultAsync(d => d.Name == rawgDev.Name);

                    if (developer == null)
                    {
                        developer = new Developer { Name = rawgDev.Name };
                        _context.Developers.Add(developer);
                        await _context.SaveChangesAsync();
                    }
                }

                var game = new Game
                {
                    Title = details.Name,
                    RawgGameId = details.Id,
                    DeveloperId = developer?.Id ?? 0,
                    IsPublished = false
                };

                foreach (var rawgGenre in details.Genres)
                {
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == rawgGenre.Name);

                    if (category == null)
                    {
                        category = new Category { Name = rawgGenre.Name };
                        _context.Categories.Add(category);
                        await _context.SaveChangesAsync();
                    }

                    game.GameCategories.Add(new GameCategory
                    {
                        CategoryId = category.Id
                    });
                }

                _context.Games.Add(game);
                await _context.SaveChangesAsync();
            }
        }
    }
}
