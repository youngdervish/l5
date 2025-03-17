using Microsoft.AspNetCore.Identity;
using l5.Models;

namespace l5.Services
{
    public class TokenService
    {
        private readonly string _secretKey;
        private readonly UserManager<User> _userManager;
    }
}
