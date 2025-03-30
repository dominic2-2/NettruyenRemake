using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;
using System.Threading.Tasks;
using BCrypt.Net;

namespace NettruyenRemake.Controllers
{
    public class AccountController : Controller
    {
        private readonly NettruyenDbContext _context;

        public AccountController(NettruyenDbContext context)
        {
            _context = context;
        }

        // GET: Account
        public async Task<IActionResult> Index()
        {
            // Check if user is admin
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3) // 3 = admin role from DB
            {
                return RedirectToAction("Index", "Home");
            }

            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return View(users);
        }

        // GET: Account/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Check if user is admin
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Account/Create
        public IActionResult Create()
        {
            // Check if user is admin
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Username, string Email, string PasswordHash, int? RoleId)
        {
            // Check if user is admin
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            // Create new user object
            var user = new User
            {
                Username = Username,
                Email = Email,
                PasswordHash = PasswordHash,
                RoleId = RoleId ?? 0 // Use 0 if null to trigger validation error
            };

            // Validation
            if (string.IsNullOrEmpty(Username))
            {
                ModelState.AddModelError("Username", "Username is required");
            }
            else if (await _context.Users.AnyAsync(u => u.Username == Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
            }

            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError("Email", "Email is required");
            }
            else if (await _context.Users.AnyAsync(u => u.Email == Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
            }

            if (string.IsNullOrEmpty(PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Password is required");
            }

            if (RoleId == null || RoleId <= 0)
            {
                ModelState.AddModelError("RoleId", "Please select a valid role");
            }
            else if (!await _context.Roles.AnyAsync(r => r.RoleId == RoleId))
            {
                ModelState.AddModelError("RoleId", "Selected role does not exist");
            }

            // Debug information
            System.Diagnostics.Debug.WriteLine($"Username: {Username}");
            System.Diagnostics.Debug.WriteLine($"Email: {Email}");
            System.Diagnostics.Debug.WriteLine($"RoleId: {RoleId}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the password
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash);
                    user.CreatedAt = DateTime.Now;

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Account created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the error
                    ModelState.AddModelError("", "An error occurred while creating the account: " + ex.Message);
                }
            }

            // If we got this far, something failed; redisplay form
            ViewBag.Roles = await _context.Roles.ToListAsync();
            
            // Debug ModelState errors
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {key}, Errors: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            return View(user);
        }
        // GET: Account/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3) // Check if admin
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if admin is trying to edit their own account
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot edit your own account for security reasons.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }

        // POST: Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Username, string Email, string? newPassword, int RoleId)
        {
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3) // Check if admin
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if admin is trying to edit their own account
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot edit your own account for security reasons.";
                return RedirectToAction(nameof(Index));
            }

            // Check required fields
            if (string.IsNullOrEmpty(Username))
            {
                ModelState.AddModelError("Username", "Username is required.");
            }
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            if (RoleId == 0)
            {
                ModelState.AddModelError("RoleId", "Please select a role.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check for duplicate username (excluding current user)
            if (await _context.Users.AnyAsync(u => u.Username == Username && u.UserId != id))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
            }

            // Check for duplicate email (excluding current user)
            if (await _context.Users.AnyAsync(u => u.Email == Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);
                return View(user);
            }

            // Update user properties
            user.Username = Username;
            user.Email = Email;
            user.RoleId = RoleId;

            // Only update password if newPassword is provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
            // If newPassword is blank or null, current password remains unchanged

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Account updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating account: {ex.Message}");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);
                return View(user);
            }
        }
        // GET: Account/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if user is admin
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            // Prevent deleting your own account
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account for security reasons.";
                return RedirectToAction(nameof(Index));
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if user is admin
            int? userRoleId = HttpContext.Session.GetInt32("UserRole");
            if (userRoleId != 3)
            {
                return RedirectToAction("Index", "Home");
            }

            // Prevent deleting your own account
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
} 