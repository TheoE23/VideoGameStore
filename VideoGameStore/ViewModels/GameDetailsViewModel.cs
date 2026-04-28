using VideoGameStore.Models;

namespace VideoGameStore.ViewModels
{
    public class GameDetailsViewModel
    {
        public Game Game { get; set; }
        public List<Review> Reviews { get; set; }

        public string NewReviewContent { get; set; }
        public int NewReviewRating { get; set; }

        public List<Purchase> Purchases { get; set; } = new();
    }
}
