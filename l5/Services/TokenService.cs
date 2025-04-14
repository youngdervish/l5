using Microsoft.AspNetCore.Identity;
using l5.Core.Interfaces;
using l5.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace l5.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TokenService> _logger;
        public TokenService(IConfiguration configuration, UserManager<User> userManager, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            var secretKey = _configuration["JwtSettings:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            string jti = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("jti", jti)
            };

            var accessTokenExpiry = DateTime.Now.AddMinutes(3);
            var accessToken = new JwtSecurityToken(
                issuer: "Erkin",
                audience: "Bazaar",
                claims: claims,
                expires: accessTokenExpiry,
                signingCredentials: credentials
            );

            _logger.LogInformation($"Access token was generated for the user: {user.UserName}");
            return new JwtSecurityTokenHandler().WriteToken(accessToken);
        }

        public async Task<(string RefreshToken, DateTime Expiry)> GenerateRefreshToken(string userId)
        {
            var refreshToken = Guid.NewGuid().ToString();
            var expiry = DateTime.Now.AddMinutes(5);
            var user = await _userManager.FindByIdAsync(userId);

            _logger.LogInformation($"Refresh token was generated for the user: {user.UserName}");
            return (refreshToken, expiry);
        }

        public async Task RevokeRefreshToken(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                _logger.LogInformation($"Revoking refresh token for user {user.UserName}");
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _userManager.UpdateAsync(user);
            }
            else { _logger.LogWarning($"Failed to revoke the refresh token. User NOT found!!!"); }
        }

        public string GetAccessTokenExpiry(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);
            
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (usernameClaim != null)
            {
                string username = usernameClaim.Value;
                _logger.LogInformation($"Username is extracted for acquiring the Access Token Expiry for the user: {username}");
            }
            else _logger.LogError("Username NOT found in the access token for acquiring the Access Token Expiry");

            return jwtToken.ValidTo.ToString();
        }

        public string GetUserViaAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (usernameClaim != null)
            {
                string username = usernameClaim.Value;
                _logger.LogInformation($"Username is extracted for acquiring the Username via Access Token for the user: {username}");
            }
            else _logger.LogError("Username NOT found in the access token for acquiring the Username via Access Token");

            return jwtToken?.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
        }

        public async Task<(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry)> RotateTokens(string refreshToken)
        {
            var users = _userManager.Users;
            var user = await users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null) throw new SecurityTokenException("Invalid or expired Refresh Token");

            var accessToken = GenerateAccessToken(user);
            var (newRefreshToken, newRefreshTokenExpiry) = await GenerateRefreshToken(user.Id);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = newRefreshTokenExpiry;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Access and Refresh Tokens were Rotated for the user: {user.UserName}");
            return (accessToken, newRefreshToken, newRefreshTokenExpiry);
        }

        public bool AccessTokenExpiryCheck(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);
            var expiryClaim = jwtToken?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Expiration)?.Value;
            if (expiryClaim == null) return false;
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (usernameClaim != null)
            {
                string username = usernameClaim.Value;
                _logger.LogInformation($"Access Token Expiry check was performed for user: {username} on {DateTime.UtcNow}");
            }
            else _logger.LogError("Username NOT found in the access token");

            DateTime tokenExpiryTime = DateTime.Parse(expiryClaim);
            return tokenExpiryTime >= DateTime.UtcNow.AddSeconds(30);
        }
        public bool RefreshTokenExpiryCheck(User user)
        {
            if (user.RefreshTokenExpiry == null) return false;
            _logger.LogInformation($"Refresh Token Expiry check was performed for the user: {user.UserName}");
            return user.RefreshTokenExpiry <= DateTime.UtcNow.AddSeconds(30);
        }
    }
}

