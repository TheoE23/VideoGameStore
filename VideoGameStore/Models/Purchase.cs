namespace VideoGameStore.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public decimal Price { get; set; }

        public string PayPalOrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
