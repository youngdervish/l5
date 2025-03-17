using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using l5.Models;

namespace l5.Pages
{
    public class Dashboard : PageModel
    {
        public string UserRole { get; set; } = "Guest";  // Default role

        public IActionResult OnGet()
        {
            UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "Guest";
            return Page();
        }
    }
}
