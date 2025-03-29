using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;
using BCrypt.Net;
using NettruyenRemake.Helpers;

namespace NettruyenRemake.Controllers
{
    public class AuthenController : Controller
    {
        private readonly NettruyenDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenController(NettruyenDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> FixPasswords()
        //{
        //    var users = _context.Users.ToList();

        //    foreach (var user in users)
        //    {
        //        // Check if password is already hashed (a valid bcrypt hash starts with "$2a$", "$2b$", or "$2y$")
        //        if (!user.PasswordHash.StartsWith("$2"))
        //        {
        //            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Hash only if it's plain text
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    return Content("Password hashes updated successfully!");
        //}
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

        public IActionResult Register()
        {
            ViewBag.account = new User();
            return View();
        }

        [HttpPost]
        public IActionResult Save(User user)
        {
            // Check if the username already exists
            var existingUsername = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUsername != null)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View("Register", user);
            }

            // Check if the email already exists
            var existingEmail = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View("Register", user);
            }

            try
            {
                // Hash the password
                user.PasswordHash = BCrypt.Net.BCrypt.HashString(user.PasswordHash.Trim());

                // Generate security code
                string securityCode = RandomHelper.RandomString(6);
                DateTime codeCreatedTime = DateTime.Now;

                // Store all registration data in session
                HttpContext.Session.SetString("PendingUsername", user.Username);
                HttpContext.Session.SetString("PendingEmail", user.Email);
                HttpContext.Session.SetString("PendingPasswordHash", user.PasswordHash);
                HttpContext.Session.SetString("SecurityCode", securityCode);
                HttpContext.Session.SetString("SecurityCodeCreatedAt", codeCreatedTime.ToString("o"));

                // Set the default role (assuming 1 is the normal user role)
                HttpContext.Session.SetInt32("PendingRoleId", 1);

                // Send activation email
                var mailHelper = new MailHelper(_configuration);
                string content = "Security Code: " + securityCode;
                mailHelper.Send(_configuration["Gmail:Username"], user.Email, "Activate Your Account", content);

                return RedirectToAction("Active");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View("Register", user);
            }
        }

        public IActionResult Active()
        {
            ViewBag.username = HttpContext.Session.GetString("PendingUsername");
            return View("Active");
        }

        public IActionResult Resend()
        {
            var email = HttpContext.Session.GetString("PendingEmail");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            // Generate new security code
            string securityCode = RandomHelper.RandomString(6);
            HttpContext.Session.SetString("SecurityCode", securityCode);
            HttpContext.Session.SetString("SecurityCodeCreatedAt", DateTime.Now.ToString("o"));

            // Send new activation email
            var mailHelper = new MailHelper(_configuration);
            string content = "Security Code: " + securityCode;
            mailHelper.Send(_configuration["Gmail:Username"], email, "Active Account By Email", content);

            return RedirectToAction("Active");
        }

        [HttpPost]
        public IActionResult Active(string securityCode)
        {
            var storedSecurityCode = HttpContext.Session.GetString("SecurityCode");
            var createdAtString = HttpContext.Session.GetString("SecurityCodeCreatedAt");

            // Check if session data exists
            if (string.IsNullOrEmpty(storedSecurityCode) || string.IsNullOrEmpty(createdAtString))
            {
                return RedirectToAction("register");
            }

            DateTime createdAt = DateTime.Parse(createdAtString);
            int seconds = DateTime.Now.Subtract(createdAt).Seconds;

            if (storedSecurityCode == securityCode && seconds <= 120)
            {
                try
                {
                    // Get all user data from session
                    var username = HttpContext.Session.GetString("PendingUsername");
                    var email = HttpContext.Session.GetString("PendingEmail");
                    var passwordHash = HttpContext.Session.GetString("PendingPasswordHash");
                    var roleId = HttpContext.Session.GetInt32("PendingRoleId") ?? 1;

                    // Create and save the user to database
                    var user = new User
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = passwordHash,
                        CreatedAt = DateTime.Now,
                        RoleId = roleId
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    // Clear all registration-related session data
                    HttpContext.Session.Remove("PendingUsername");
                    HttpContext.Session.Remove("PendingEmail");
                    HttpContext.Session.Remove("PendingPasswordHash");
                    HttpContext.Session.Remove("PendingRoleId");
                    HttpContext.Session.Remove("SecurityCode");
                    HttpContext.Session.Remove("SecurityCodeCreatedAt");

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving user: {ex.Message}");
                    ViewBag.msg = "An error occurred while creating your account.";
                    ViewBag.username = HttpContext.Session.GetString("PendingUsername");
                    return View("Active");
                }
            }
            else
            {
                ViewBag.username = HttpContext.Session.GetString("PendingUsername");
                ViewBag.msg = "Invalid or expired security code. Please try again or request a new code.";
                return View("Active");
            }
        }
    }
}
