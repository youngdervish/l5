using l5.Core.Models;
using l5.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace l5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        public JWTController(IConfiguration configuration, UserManager<User> userManager, TokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost("rotate-tokens")]
        public async Task<IActionResult> RotateTokens()//[FromBody] RotateTokenRequest request)
        {
            var oldRefreshToken = Request.Cookies["refreshToken"];

            try
            {
                var (accessToken, newRefreshToken, newRefreshTokenExpiry) = await _tokenService.RotateTokens(oldRefreshToken);
                Response.Cookies.Append("accessToken", accessToken, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddMinutes(30)});
                Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions { HttpOnly = true, Expires = newRefreshTokenExpiry});
                return Ok(new {Message = "Tokens rotated successfully" });
            }
            catch (Exception ex)
            {
                return Unauthorized(new {Error = ex.Message});
            }
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
