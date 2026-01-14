using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VideoGameStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = "";
    }
}
