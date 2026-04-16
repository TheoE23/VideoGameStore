using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;

namespace VideoGameStore.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(int gameId, string userId, string content, int rating)
        {
            var review = new Review
            {
                GameId = gameId,
                UserId = userId,
                Content = content,
                Rating = rating
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Review>> GetGameReviewsAsync(int gameId)
        {
            return await _context.Reviews
                .Where(r => r.GameId == gameId && !r.IsDeleted)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task ReportReviewAsync(int reviewId, string userId, string reason)
        {
            var report = new ReviewReport
            {
                ReviewId = reviewId,
                ReporterId = userId,
                Reason = reason
            };

            _context.ReviewReports.Add(report);
            await _context.SaveChangesAsync();
        }
    }
}
