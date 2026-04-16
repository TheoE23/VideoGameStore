using System.ComponentModel.DataAnnotations;

namespace VideoGameStore.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public int GameId { get; set; }
        public Game Game { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<ReviewReport> Reports { get; set; }
    }
}
