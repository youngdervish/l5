using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using l5.Data;
using l5.Models;
using l5.DTOs;
using Microsoft.AspNetCore.Identity;


namespace l5.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Select(u => new UserDTO
                    {
                        Username = u.UserName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Role = u.Role
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUser(string username)
        {
            var users = await _context.Users.Where(u => u.UserName.Contains(username)).ToListAsync();
            if (users == null || !users.Any()) return NotFound("No users exist matching the search query");

            var results = users.Select(user => new UserDTO
            {
                Username = user.UserName,
                Role = user.Role,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            }).ToList();

            return Ok(results);
        }

        [HttpPost("add-user")]
        public async Task<ActionResult<UserDTO>> AddUser([FromBody] User user, [FromQuery] string password)
        {
            if (await _userManager.FindByNameAsync(user.UserName) != null)
                return BadRequest("Username already exists");

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!string.IsNullOrEmpty(user.Role))
            {
                if (!await _roleManager.RoleExistsAsync(user.Role))
                    await _roleManager.CreateAsync(new IdentityRole(user.Role));
                await _userManager.AddToRoleAsync(user, user.Role);
            }

            var userDTO = new UserDTO()
            {
                Username = user.UserName,
                Role = user.Role,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, userDTO);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] User updatedUser)
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser == null) return NotFound();

            bool isUpdated = false;

            // Update Email
            if (!string.IsNullOrEmpty(updatedUser.Email) && updatedUser.Email != existingUser.Email)
            {
                existingUser.Email = updatedUser.Email;
                isUpdated = true;
            }

            // Update PhoneNumber
            if (!string.IsNullOrEmpty(updatedUser.PhoneNumber) && updatedUser.PhoneNumber != existingUser.PhoneNumber)
            {
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                isUpdated = true;
            }

            // Update Role
            if (!string.IsNullOrEmpty(updatedUser.Role))
            {
                existingUser.Role = updatedUser.Role;
                isUpdated = true;
            }

            // Update Password if provided
//            Console.WriteLine("\n\n Updated Password: " + updatedUser.PasswordHash);

            if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                // Decode the password to ensure no issues with encoded characters
                var decodedPassword = Uri.UnescapeDataString(updatedUser.PasswordHash);  // Decoding the encoded password

                Console.WriteLine("\n\n Updated Password: " + decodedPassword + "\n\n");
                // Hash the decoded password
                var passwordHasher = new PasswordHasher<User>();
                var hashedPassword = passwordHasher.HashPassword(existingUser, decodedPassword);

                // Update the user's password hash
                existingUser.PasswordHash = hashedPassword;
                isUpdated = true;
            }

            // If no changes were made, return 304 Not Modified
            if (!isUpdated) return StatusCode(304);

            // Save changes
            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] List<string> usernames)
        {
            var usersToDelete = _userManager.Users.Where(u => usernames.Contains(u.UserName)).ToList();
            if (!usersToDelete.Any()) return NotFound();

            foreach (var user in usersToDelete)
            {
                await _userManager.DeleteAsync(user);
            }

            return NoContent();
        }

        [HttpGet("getUsername")]
        public IActionResult GetUsername()
        {
            var username = User.Identity.Name; // Get the username from the authenticated user
            if (username == null)
            {
                return Unauthorized(); // Return 401 if the user is not logged in
            }

            return Ok(new { username });
        }

        [HttpGet("getUserRole")]
        public IActionResult GetUserRole()
        {
            var username = User.Identity.Name;
            var role = _context.Users
                                .Where(u => u.UserName == username)
                                .Select(u => u.Role)  // Assuming your role is stored in the User table
                                .FirstOrDefault();
            Console.WriteLine($"The user Role is {role}");
            if (role == null)
            {
                return Unauthorized("User role not found.");
            }
            return Ok(new { role });
        }
    }
}
