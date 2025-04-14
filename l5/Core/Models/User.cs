using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace l5.Core.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
