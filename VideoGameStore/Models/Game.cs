namespace VideoGameStore.Models
{
    public class Game
    {
        public int Id { get; set; }

        // External API reference
        public int? RawgGameId { get; set; }

        // Cached metadata for catalog display
        public string Title { get; set; } = string.Empty;

        // Catalog controls
        public bool IsPublished { get; set; } = true;
        public decimal? Price { get; set; }

        // Developer relationship
        public int DeveloperId { get; set; }
        public Developer Developer { get; set; } = null!;

        // Category relationship
        public ICollection<GameCategory> GameCategories { get; set; } = new List<GameCategory>();
    }
}
