using System.ComponentModel.DataAnnotations;

namespace l5.Application.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; }
        [Required]
        public string Password { get; set; } 
        public string Role { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
