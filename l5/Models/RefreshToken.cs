using System.ComponentModel.DataAnnotations;

namespace l5.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }
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
