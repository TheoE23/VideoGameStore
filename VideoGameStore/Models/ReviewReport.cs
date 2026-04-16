using System.ComponentModel.DataAnnotations;

namespace VideoGameStore.Models
{
    public class ReviewReport
    {
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        public int ReviewId { get; set; }
        public Review Review { get; set; }

        public string ReporterId { get; set; }
        public ApplicationUser Reporter { get; set; }

        public string? ResolvedByAdminId { get; set; }
        public ApplicationUser? ResolvedByAdmin { get; set; }
    }
}
