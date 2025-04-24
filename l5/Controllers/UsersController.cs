using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using l5.Core.Models;
using l5.Application.DTOs;
using l5.Core.Interfaces;



namespace l5.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetUsersAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Search(string username)
        {
            var users = await _userService.SearchUsersAsync(username);
            //if (users == null || !users.Any()) return NotFound("No users exist matching the search query");

            return Ok(users);
        }

        [HttpPost("add-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> AddUser([FromBody] CreateUserDTO createUserDto)
        {
            try
            {
                var createdUser = await _userService.AddUserAsync(createUserDto);
                //if (createdUser == null)
                //    return BadRequest("Username already exists or user creation failed.");
                return Ok(createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(username, updateUserDto);
                //var result = await _userService.UpdateUserAsync(username, updateUserDto);
                //if (!result) return NotFound("User not found or no changes detected.");

                return Ok(updateUserDto);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpDelete("delete-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromBody] List<string> usernames)
        {
            var result =  await _userService.DeleteUserAsync(usernames);
            return result ? Ok("Users deleted successfully") : NotFound("No user deleted");
        }

        [HttpGet("getUsername")]
        [Authorize]
        public IActionResult GetUsername()
        {
            var username = _userService.GetUsername(User);
            if (string.IsNullOrEmpty(username))
                return NotFound("Username not found.");
            return Ok(new { username });
        }

        [HttpGet("getUserRole")]
        [Authorize]
        public IActionResult GetUserRole()
        {
            var role = _userService.GetUserRole(User);
            return Ok(new { role });
        }
    }
}
