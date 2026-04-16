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

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}