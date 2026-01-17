namespace VideoGameStore.Models
{
    public class Game
    {
        public int Id { get; set; }

        public int? RawgGameId { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;
        public decimal? Price { get; set; }

        public int DeveloperId { get; set; }
        public Developer? Developer { get; set; } = null!;

        public ICollection<GameCategory> GameCategories { get; set; } = new List<GameCategory>();
    }
}
