namespace VideoGameStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<GameCategory> GameCategories { get; set; } = new List<GameCategory>();
    }
}
