    using Microsoft.AspNetCore.Mvc;
    using NettruyenRemake.Models;

    public class ProfileController : Controller
    {
        private readonly NettruyenDbContext _context;

        public ProfileController(NettruyenDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Authen");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Authen");
            }
            HttpContext.Session.Set("UserAvatar", user.Avatar);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, IFormFile avatarFile, [Bind(Prefix = "Password")] string newPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Authen");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Authen");
            }

            user.Username = model.Username;
            user.Email = model.Email;

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            if (avatarFile != null && avatarFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await avatarFile.CopyToAsync(memoryStream);
                    user.Avatar = memoryStream.ToArray();
                }
            }


            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("UserName", user.Username);
                return RedirectToAction(nameof(Edit));
                //ViewBag.Message = "Account updated successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to update account. Please try again.";
                ModelState.AddModelError(string.Empty, "Unable to update account. Please try again.");
            }

            return View(user);
        }

    public IActionResult Avatar()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return File("~/images/default-avatar.jpg", "image/jpeg");
        }

        var user = _context.Users.Find(userId);
        if (user?.Avatar != null)
        {
            return File(user.Avatar, "image/jpeg");
        }
        return File("~/images/default-avatar.jpg", "image/jpeg");
    }
}