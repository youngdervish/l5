using l5.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace l5.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        Task<(string RefreshToken, DateTime Expiry)> GenerateRefreshToken(string userId);
        string GetAccessTokenExpiry(string accessToken);
        string GetUserViaAccessToken(string accessToken);
        Task RevokeRefreshToken(string userId);
        Task<(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry)> RotateTokens(string accessToken);
        public bool AccessTokenExpiryCheck(string accessToken);
        public bool RefreshTokenExpiryCheck(User user);
        
    }
}
