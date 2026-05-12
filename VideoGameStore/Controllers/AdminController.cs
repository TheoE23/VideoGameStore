using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;
using VideoGameStore.Services.Games;
using VideoGameStore.Services.Reviews;

namespace VideoGameStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ReviewReports()
        {
            var reports = await _context.ReviewReports
                .Include(r => r.Review)
                    .ThenInclude(r => r.User)
                .Include(r => r.Reporter)
                .Where(r => !r.IsResolved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var report = await _context.ReviewReports.FindAsync(id);

            if (report == null)
                return NotFound();

            report.IsResolved = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReviewReports));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return NotFound();

            review.IsDeleted = true;

            var relatedReports = await _context.ReviewReports
                .Where(r => r.ReviewId == reviewId && !r.IsResolved)
                .ToListAsync();

            foreach (var report in relatedReports)
            {
                report.IsResolved = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReviewReports));
        }

        public async Task<IActionResult> Users(int page = 1)
        {
            const int pageSize = 10;

            var query = _userManager.Users
                .OrderBy(u => u.Email)
                .AsQueryable();

            var totalUsers = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;

            ViewBag.TotalPages =
                (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.IsBanned = true;

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.IsBanned = false;

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Users));
        }
    }
}