using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NettruyenRemake.Controllers
{
    public class AuthenController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Please provide both email and password.";
                return View();
            }

            // Hardcoded user (Replace this with database logic later)
            var hardcodedUser = new
            {
                AccountId = 1,
                AccountName = "JohnDoe",
                Email = "test@example.com",
                Password = "password123",
                AccountRole = 1
            };

            if (email != hardcodedUser.Email || password != hardcodedUser.Password)
            {
                ViewBag.Message = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", hardcodedUser.AccountId);
            HttpContext.Session.SetString("UserName", hardcodedUser.AccountName);
            HttpContext.Session.SetInt32("UserRole", hardcodedUser.AccountRole);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using NettruyenRemake.Models; 

//namespace NettruyenRemake.Controllers
//{
//    public class AuthenController : Controller
//    {
//        private readonly ApplicationDbContext _context; 

//        public AuthenController(ApplicationDbContext context)
//        {
//            _context = context; 
//        }

//        public IActionResult Login()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Login(string email, string password)
//        {
//            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
//            {
//                ViewBag.Message = "Please provide both email and password.";
//                return View();
//            }

            
//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            
//            if (user == null || user.PasswordHash != password)
//            {
//                ViewBag.Message = "Invalid email or password.";
//                return View();
//            }

           
//            HttpContext.Session.SetInt32("UserId", user.UserId);
//            HttpContext.Session.SetString("UserName", user.Username); 
//            HttpContext.Session.SetInt32("UserRole", user.RoleId); 

//            return RedirectToAction("Index", "Home");
//        }

//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToAction("Index", "Home");
//        }
//    }
//}