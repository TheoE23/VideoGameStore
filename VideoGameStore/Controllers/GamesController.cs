using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;
using VideoGameStore.Services.Games;

namespace VideoGameStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGameImportService _gameImportService;

        public GamesController(ApplicationDbContext context, IGameImportService gameImportService)
        {
            _context = context;
            _gameImportService = gameImportService;
        }

        public async Task<IActionResult> Index()
        {
            var games = await _context.Games
                .Include(g => g.Developer)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .ToListAsync();

            ViewBag.Developers = await _context.Developers.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(games);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Game game, int[] selectedCategories)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Developers = await _context.Developers.ToListAsync();
                ViewBag.Categories = await _context.Categories.ToListAsync();

                var games = await _context.Games
                    .Include(g => g.Developer)
                    .Include(g => g.GameCategories)
                        .ThenInclude(gc => gc.Category)
                    .ToListAsync();

                return View("Index", games);
            }

            foreach (var categoryId in selectedCategories)
            {
                game.GameCategories.Add(new GameCategory
                {
                    CategoryId = categoryId
                });
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Game game, int[] selectedCategories)
        {
            var existingGame = await _context.Games
                .Include(g => g.GameCategories)
                .FirstOrDefaultAsync(g => g.Id == game.Id);

            if (existingGame == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            existingGame.Title = game.Title;
            existingGame.Price = game.Price;
            existingGame.IsPublished = game.IsPublished;
            existingGame.DeveloperId = game.DeveloperId;

            existingGame.GameCategories.Clear();
            foreach (var categoryId in selectedCategories)
            {
                existingGame.GameCategories.Add(new GameCategory
                {
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTopGames()
        {
            await _gameImportService.ImportTopGamesAsync(50);

            return RedirectToAction(nameof(Index));
        }
    }
}
