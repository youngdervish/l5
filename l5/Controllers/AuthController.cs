using Microsoft.AspNetCore.Mvc;
using l5.Models;
using l5.Data;
using l5.DTOs;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace l5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWTController _jwtController;
        public AuthController(AppDbContext context, IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, JWTController jwtController)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtController = jwtController;
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

                //var token = GenerateJwtToken(user);                
                var accessToken = _jwtController.GenerateAccessToken(user);
                //var refreshToken = GenerateRefreshToken();

                (user.RefreshToken, user.RefreshTokenExpiry) = _jwtController.GenerateRefreshToken();

                //user.RefreshToken = refreshToken;
                //user.RefreshTokenExpiry = DateTime.Now.AddMinutes(2);
                await _userManager.UpdateAsync(user);

                Response.Cookies.Append("accessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddMinutes(1)
                });

                Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions 
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,                                        //can i switch to strict without breaking the code?
                    Expires = user.RefreshTokenExpiry
                });

                //var timeLeft = user.RefreshTokenExpiry - DateTime.Now;                  //for chkn the refresh token leftover on sign in
                //Console.WriteLine("Date Time Now is " + DateTime.Now + "Time left on refresh token: " + timeLeft);           //print the refresh token leftover on sign in
                //Console.WriteLine("Access Token: " + token.Item1);                      //print the access token on sign in

                return Ok(new
                {
                    AccessToken = accessToken,
                    //RefreshToken = refreshToken,
                    //TimeLeftOnRefreshToken = timeLeft,
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
            var accessToken = Request.Cookies["accessToken"];
            Console.WriteLine("Access Token Check: \n\n" + accessToken.ToString() + "\n\n");

            if(string.IsNullOrEmpty(accessToken))
            { 
                Console.WriteLine("No access tokens were found");
                return Unauthorized(new { message = "No access token provided" });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);

                var username = jwt?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("Username NOT found!!!");
                    return Unauthorized(new { message = "Username isse with the token, can be expired as well" });
                }

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    Console.WriteLine("FAIL!!!User NOT found.");
                    return BadRequest(new { message = "User NOT found or already LOGGED OUT!!!" });
                }

                Console.WriteLine($"\nLogging out user: {username} i.e. {user.UserName}\n");

                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _userManager.UpdateAsync(user);

                Response.Cookies.Delete("accessToken");
                Response.Cookies.Delete("refreshToken");

                Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "-1";
                return Ok(new { message = "Log Out successful" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "Stuff went down", ex.Message });
            }
        }
    }
}

#region v1


//private Tuple<string, string, DateTime> GenerateJwtToken(User user)
//{
//    var secretKey = _configuration["JwtSettings:Secret"];
//    Console.WriteLine($"Secret Key: {secretKey}");

//    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
//    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//    var claims = new[]
//    {
//        new Claim(ClaimTypes.Name, user.UserName),
//        new Claim(ClaimTypes.Role, user.Role)
//    };

//    var accessTokenExpiry = DateTime.Now.AddSeconds(20);
//    var refreshTokenExpiry = DateTime.Now.AddSeconds(30);

//    var accessToken = new JwtSecurityToken(
//        issuer: "Erkin",
//        audience: "Bazar",
//        claims: claims,
//        expires: accessTokenExpiry,
//        signingCredentials: credentials);

//    var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
//    var refreshToken = Guid.NewGuid().ToString();

//    return Tuple.Create(accessTokenString, refreshToken, refreshTokenExpiry);
//}

//[HttpPost("refresh-token")]
//public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
//{
//    var user = await _userManager.FindByNameAsync(request.UserName);
//    if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.Now)
//    {
//        return Unauthorized("Invalid refresh token or expired.");
//    }

//    var (newAccessToken, newRefreshToken, newRefreshTokenExpiry) = GenerateJwtToken(user);

//    // Update refresh token and expiry in the database
//    user.RefreshToken = newRefreshToken;
//    user.RefreshTokenExpiry = DateTime.Now.AddMinutes(10); // Set a new expiry for the refresh token
//    await _userManager.UpdateAsync(user);


//    Console.WriteLine("Current Refresh Token: " + newRefreshToken);
//    return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
//}

//private User Authenticate(string username, string password)
//{
//    var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.Password == password);
//    //var user = _context.Users.FirstOrDefault(u => u.Username == username);
//    //Console.WriteLine(user == null ? "User not found" : $"Authenticated User : {user.Username}");

//    if (user == null) { return null; }// || !BCrypt.Net.BCrypt.Verify(password, user.Password)) { return null; }
//    return user;
//}

//[Authorize(Roles = "Administrator")]
//[HttpGet("admin-resource")]
//public IActionResult GetAdminResource() { return Ok("This is accessible only by Administrators"); }

//[Authorize(Roles = "User")]
//[HttpGet("user-resource")]
//public IActionResult GetUserResource() { return Ok("This is accessible only by regular users"); }

//[HttpGet("test-db")]
//public IActionResult TestDatabaseConnection()
//{
//    try
//    {
//        // Try to retrieve a user from the database
//        var user = _context.Users.FirstOrDefault();

//        if (user == null)
//        {
//            return NotFound(new { message = "No users found in the database." });
//        }

//        return Ok(new { message = "Database connection is successful.", user = user.Username });
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, new { message = "Database connection failed.", details = ex.Message });
//    }
//}

#endregion