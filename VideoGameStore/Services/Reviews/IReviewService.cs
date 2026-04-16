using VideoGameStore.Models;

namespace VideoGameStore.Services.Reviews
{
    public interface IReviewService
    {
        Task AddReviewAsync(int gameId, string userId, string content, int rating);
        Task<IEnumerable<Review>> GetGameReviewsAsync(int gameId);
        Task ReportReviewAsync(int reviewId, string userId, string reason);
    }
}
