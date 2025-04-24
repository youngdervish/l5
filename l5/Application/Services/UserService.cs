using AutoMapper;
using AutoMapper.QueryableExtensions;
using l5.Application.DTOs;
using l5.Core.Interfaces;
using l5.Core.Models;
using l5.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace l5.Application.Services
{
    public class UserService : IUserService
    {
        //private readonly AppDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        //private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<User> userManager, IMapper mapper, ILogger logger)//AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            //_context = context;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            //_roleManager = roleManager;
        }

        public async Task<List<UserDTO>> GetUsersAsync()
        {
            //return await _userManager.Users.Select(u => new UserDTO
            //{
            //    Username = u.UserName,
            //    Email = u.Email,
            //    PhoneNumber = u.PhoneNumber,
            //    Role = u.Role
            //}).ToListAsync();

            return await _mapper.ProjectTo<UserDTO>(_userManager.Users).ToListAsync();
        }

        public async Task<List<UserDTO>> SearchUsersAsync(string username)
        {
            //return await _context.Users.Where(u => u.UserName.Contains(username)).Select(user => new UserDTO
            //{
            //    Username = user.UserName,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber,
            //    Role = user.Role
            //}).ToListAsync();

            // above vs below?

            //var users = await _userManager.Users.Where(u => u.UserName.Contains(username)).Select(user => new UserDTO
            //{
            //    Username = user.UserName,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber,
            //    Role = user.Role
            //}).ToListAsync();

            var users = await _userManager.Users
                .Where(u => u.UserName.Contains(username))
                .ProjectTo<UserDTO>(_mapper.ConfigurationProvider).ToListAsync();

            if (users == null || !users.Any())
                throw new Exception("No users exist matching the search query.");

            return users;
        }

        public async Task<UserDTO?> AddUserAsync(User user, string password)
        {
            var existingUser = await _userManager.FindByNameAsync(user.UserName);
            if (existingUser != null)
                throw new Exception("Username already exists.");

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errors}");
            }
            
            await _userManager.AddToRoleAsync(user, user.Role);

            return _mapper.Map<UserDTO>(user);

            //return new UserDTO
            //{

            //    Username = user.UserName,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber,
            //    Role = user.Role
            //};
        }
        public async Task<UserDTO?> AddUserAsync(CreateUserDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return null;
            return _mapper.Map<UserDTO>(user);

            //var user = new User
            //{
            //    UserName = dto.Username,
            //    Email = dto.Email,
            //    PhoneNumber = dto.PhoneNumber,
            //    Role = dto.Role
            //};

            //return await AddUserAsync(user, dto.Password);
        }

        public async Task<bool> UpdateUserAsync(string username, User updatedUser)
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser == null) return false;

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
            // Console.WriteLine("\n\n Updated Password: " + updatedUser.PasswordHash);

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
            if (!isUpdated) return false;

            // Save changes
            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded;
        }
        public async Task<UserDTO?> UpdateUserAsync(string username, UpdateUserDTO dto)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception("User not found.");

            // Update only non-null fields
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Role)) user.Role = dto.Role;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"User update failed: {errors}");
            }

            return _mapper.Map<UserDTO>(user);

            //return new UserDTO
            //{
            //    Username = user.UserName,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber,
            //    Role = user.Role
            //};
        }

        public async Task<bool> DeleteUserAsync(List<string> usernames)
        {
            var usersToDelete = await _userManager.Users.Where(u => usernames.Contains(u.UserName)).ToListAsync();
            if (!usersToDelete.Any())
            {
                _logger.LogWarning("No matches found for deletion");
                return false;
            }

            foreach (var user in usersToDelete)
            {
                var result = await _userManager.DeleteAsync(user);
                if(!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(_e => _e.Description));
                    _logger.LogError("Failed to delete user {Username}. Error: {Errors}", user.UserName, errors);
                    continue;
                    //throw new Exception($"Failed to delete user {user.UserName}: {errors}");
                }
                _logger.LogInformation("User deleted: {Username}", user.UserName);
            }
            return true;
        }

        public string? GetUsername(System.Security.Claims.ClaimsPrincipal user) { return user.Identity?.Name; }

        public string? GetUserRole(System.Security.Claims.ClaimsPrincipal user)
        {
            var username = GetUsername(user); //user.Identity?.Name;
            //return _context.Users.Where(u => u.UserName == username).Select(u => u.Role).FirstOrDefault();
            return _userManager.Users.Where(u => u.UserName == username).Select(u => u.Role).FirstOrDefault();
        }

        public string? GetUserEmail(System.Security.Claims.ClaimsPrincipal user)
        {
            var username = GetUsername(user);
            return _userManager.Users.Where(u => u.UserName == username).Select(u => u.Email).FirstOrDefault();
        }
    }
}
