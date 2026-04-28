using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;
using VideoGameStore.Services.Games;
using VideoGameStore.Services.Reviews;
using VideoGameStore.ViewModels;

namespace VideoGameStore.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGameImportService _gameImportService;
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GamesController(
            ApplicationDbContext context,
            IGameImportService gameImportService,
            IReviewService reviewService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _gameImportService = gameImportService;
            _reviewService = reviewService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
                return RedirectToAction(nameof(Index));

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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTopGames()
        {
            await _gameImportService.ImportTopGamesAsync(50);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Public(int page = 1, int? categoryId = null, int? developerId = null)
        {
            const int pageSize = 12;

            var query = _context.Games
                .Include(g => g.Developer)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(g =>
                    g.GameCategories.Any(gc => gc.CategoryId == categoryId.Value));
            }

            if (developerId.HasValue)
            {
                query = query.Where(g => g.DeveloperId == developerId.Value);
            }

            var totalGames = await query.CountAsync();

            var games = await query
                .OrderBy(g => g.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            var developers = await _context.Developers.OrderBy(d => d.Name).ToListAsync();

            var viewModel = new PublicGamesViewModel
            {
                Games = games,
                Categories = categories,
                SelectedCategoryId = categoryId,
                Developers = developers,
                SelectedDeveloperId = developerId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalGames / (double)pageSize)
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var game = await _context.Games
                .Include(g => g.Developer)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound();

            var reviews = await _reviewService.GetGameReviewsAsync(id);

            var userId = _userManager.GetUserId(User);

            var purchases = new List<Purchase>();

            if (userId != null)
            {
                purchases = await _context.Purchases
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
            }

            var vm = new GameDetailsViewModel
            {
                Game = game,
                Reviews = reviews.ToList(),
                Purchases = purchases
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(GameDetailsViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Unauthorized();

            await _reviewService.AddReviewAsync(
                model.Game.Id,
                userId,
                model.NewReviewContent,
                model.NewReviewRating
            );

            return RedirectToAction("Details", new { id = model.Game.Id });
        }

        [Authorize(Roles = "Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportReview(int reviewId, string reason)
        {
            var userId = _userManager.GetUserId(User);

            await _reviewService.ReportReviewAsync(reviewId, userId, reason);

            TempData["Success"] = "Review reported successfully.";

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}