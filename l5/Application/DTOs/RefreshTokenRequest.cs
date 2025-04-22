namespace l5.Application.DTOs
{
    public class RotateTokenRequest
    {
        public string UserName { get; set; }
        public string? RefreshToken { get; set; }
    }
}
