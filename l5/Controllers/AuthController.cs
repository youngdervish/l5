using Microsoft.AspNetCore.Mvc;
using l5.DTOs;
using Microsoft.AspNetCore.Identity;
using l5.Core.Models;
using l5.Core.Interfaces;


namespace l5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        public AuthController(UserManager<User> userManager, ITokenService tokenService, ILogger logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Username);

                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    return Unauthorized(new { message = "Invalid username or password!" });
                }

                var accessToken = _tokenService.GenerateAccessToken(user);
                (user.RefreshToken, user.RefreshTokenExpiry) = await _tokenService.GenerateRefreshToken(user.Id);
                await _userManager.UpdateAsync(user);

                Response.Cookies.Append("accessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,                                        //can i switch to strict without breaking the code?
                    Expires = DateTime.Now.AddMinutes(1)
                });

                Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions 
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,                                        //can i switch to strict without breaking the code?
                    Expires = user.RefreshTokenExpiry
                });

                _logger.LogInformation($"{user.UserName} as logged in successfully");

                return Ok(new
                {
                    AccessToken = accessToken,
                    AccessTokenExpiry = DateTime.Now.AddMinutes(1),
                    RefreshTokenExpiry = user.RefreshTokenExpiry,
                    User = new UserDTO
                    {
                        Username = user.UserName,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError(ex, "An error occurred during the login process.");
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "Internal Server Error", details = ex.Message });
            }
        }

        
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            var expiredCookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            Response.Cookies.Append("accessToken", "", expiredCookie);
            Response.Cookies.Append("refreshToken", "", expiredCookie);

            //_logger.LogInformation($"logged out successfully");             // how to get the username
            return Ok(new { message = "Logged out successfully" });
        }
    }
}