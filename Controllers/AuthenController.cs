using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;
using BCrypt.Net;

namespace NettruyenRemake.Controllers
{
    public class AuthenController : Controller
    {
        private readonly NettruyenDbContext _context;

        public AuthenController(NettruyenDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FixPasswords()
        {
            var users = _context.Users.ToList();

            foreach (var user in users)
            {
                // Check if password is already hashed (a valid bcrypt hash starts with "$2a$", "$2b$", or "$2y$")
                if (!user.PasswordHash.StartsWith("$2"))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Hash only if it's plain text
                }
            }

            await _context.SaveChangesAsync();

            return Content("Password hashes updated successfully!");
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Please provide both email and password.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Check if user exists and verify the hashed password
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Message = "Invalid email or password.";
                return View();
            }

            // Store session details
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Username);
            HttpContext.Session.SetInt32("UserRole", user.RoleId);
            if (user.Avatar != null) // If the user has an avatar, convert it
            {
                string avatarBase64 = Convert.ToBase64String(user.Avatar);
                string avatarSrc = $"data:image/png;base64,{avatarBase64}";
                HttpContext.Session.SetString("UserAvatar", avatarSrc);
            }
            else
            {
                HttpContext.Session.SetString("UserAvatar", ""); // Store an empty string for no avatar
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
