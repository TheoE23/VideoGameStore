using System.ComponentModel.DataAnnotations.Schema;

namespace VideoGameStore.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int GameId { get; set; }
        public Game Game { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}