namespace VideoGameStore.Models
{
    public class GameCategory
    {
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
