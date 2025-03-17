using l5.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace l5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        public JWTController(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }


        public string GenerateAccessToken(User user)
        {
            var secretKey = _configuration["JwtSettings:Secret"];
            //Console.WriteLine($"Secret Key: {secretKey}");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            string jti = Guid.NewGuid().ToString();
            Console.WriteLine("JTI => " + jti);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("jti", jti)
            };

            var accessTokenExpiry = DateTime.Now.AddMinutes(3);
            //Console.WriteLine($"\n\nAccess Token Expiry: {accessTokenExpiry}\n\n");

            var accessToken = new JwtSecurityToken(
                issuer: "Erkin",
                audience: "Bazar",
                claims: claims,
                expires: accessTokenExpiry,
                signingCredentials: credentials);

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
            Console.WriteLine("The NEW ACCESS TOKEN BELOW");
            Console.WriteLine(accessTokenString);
            Console.WriteLine("THE NEW ACCESS TOKEN ABOVE");

            return accessTokenString;
        }

        public (string RefreshToken, DateTime Expiry) GenerateRefreshToken()
        {
            var refreshToken = Guid.NewGuid().ToString();
            var expiry = DateTime.Now.AddMinutes(5);
            return (refreshToken, expiry);
        }

        [HttpPost("rotate-tokens")]
        public async Task<IActionResult> RotateTokens()//[FromBody] RotateTokenRequest request)
        {
            //var refreshToken = request.RefreshToken;
            var refreshToken = Request.Cookies["refreshToken"];

            Console.WriteLine($"\n\nPrinting refresh token prior to rotation: {refreshToken} \n\n");
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized("Refresh token is required.");

            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
            Console.WriteLine($"\n\nRotation request coming from user named: {user?.UserName}");
            if (user == null) return Unauthorized("Invalid refresh token");
            if (user.RefreshTokenExpiry < DateTime.Now) return Unauthorized("Refresh token has expired. Please log in again.");


            var newAccessToken = GenerateAccessToken(user);
            Console.WriteLine($"\n\nNew Rotated Access Token: {newAccessToken}\n\n");
            (user.RefreshToken, user.RefreshTokenExpiry) = GenerateRefreshToken();
            Console.WriteLine($"\n\nNew Rotated Refresh Token: {user.RefreshToken}\n\n");

            try { await _userManager.UpdateAsync(user); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating the user: {ex.Message}");
                return StatusCode(500, "An error occurred while attempting the refresh token.");
            }

            Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(3)
            });
            Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = user.RefreshTokenExpiry
            });

            Console.WriteLine("New Refresh Token: " + user.RefreshTokenExpiry);
            return Ok(new { Message = "Tokens rotated successfully." });
            //return Ok(new { AccessToken = newAccessToken , RefreshToken = user.RefreshToken });
        }

        [HttpGet("token-expiry")]
        public async Task<IActionResult> GetTokenExpiry()
        {
            var accessToken = Request.Cookies["accessToken"];
            var uName = GetUserViaAccessToken(accessToken);
            if (uName == null) return NotFound(new { message = "User UNAUTHENTICATED!!!" });
            var user = await _userManager.FindByNameAsync(uName);
            if (user == null) return NotFound(new { message = "User is NOT found!!!" });

            return Ok(new
            {
                AccessTokenExpiry = GetAccessTokenExpiry(),
                user.RefreshTokenExpiry
            });
        }

        private DateTime GetAccessTokenExpiry()
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token)) return DateTime.MinValue; // Token missing

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }

        public string GetUserViaAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(accessToken);
            var username = jwt?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username)) Console.WriteLine("Username NOT found!!!");

            return username;
        }

        [HttpGet("get-user-role-via-token")]
        public IActionResult GetRoleViaAccessToken()

        {
            var accessToken = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Access token not found in cookies.");
                return Unauthorized(new { role = "Guest" });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var userRole = jwt?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                Console.WriteLine("\n\nUserRole is: " + userRole);

                if (string.IsNullOrEmpty(userRole))
                {
                    Console.WriteLine("UserRole NOT found!!!");
                    return Ok(new { role = "Guest" });
                }

                return Ok(new { role = userRole });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error decoding token: " + ex.Message);
                return BadRequest(new { role = "Guest", error = "Invalid token" });
            }
        }

        [HttpGet("print-access-token")]
        public IActionResult PrintAccessToken()
        {
            var accessToken = Request.Cookies["accessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("No access token for you");
            }

            return Ok(new { AccessToken = accessToken });
        }

        [HttpGet("print-refresh-token")]
        public IActionResult PrintRefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token for you");
            }

            return Ok(new { RefreshToken = refreshToken });
        }
    }
}
