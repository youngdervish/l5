using l5.Application.DTOs;
using l5.Core.Models;

namespace l5.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsersAsync();
        Task<List<UserDTO>> SearchUsersAsync(string username);
        Task<UserDTO?> AddUserAsync(User user, string password);
        Task<UserDTO?> AddUserAsync(CreateUserDTO dto);
        Task<bool> UpdateUserAsync(string username, User updatedUser);
        Task<UserDTO?> UpdateUserAsync(string username, UpdateUserDTO updateUserDto);
        Task<bool> DeleteUserAsync(List<string> usernames);
        string? GetUsername(System.Security.Claims.ClaimsPrincipal user);
        string? GetUserRole(System.Security.Claims.ClaimsPrincipal user);
    }
}
