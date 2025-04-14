using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace l5.Core.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User UserObj { get; set; }

        public RefreshToken() { }
        public RefreshToken(string token, DateTime expirationDate, int userId)
        {
            Token = token;
            ExpirationTime = expirationDate;
            UserId = userId;
        }
    }
}
