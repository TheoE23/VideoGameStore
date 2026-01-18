using VideoGameStore.Models;

namespace VideoGameStore.ViewModels
{
    public class PublicGamesViewModel
    {
        public List<Game> Games { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}