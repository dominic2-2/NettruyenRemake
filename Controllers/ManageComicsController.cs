using Microsoft.AspNetCore.Mvc;
using NettruyenRemake.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NettruyenRemake.Controllers
{
    public class ManageComicsController : Controller
    {
        private readonly NettruyenDbContext _context;

        public ManageComicsController(NettruyenDbContext context)
        {
            _context = context;
        }

        // GET: ManageComics
        public async Task<IActionResult> Index()
        {
            var comics = await _context.Comics
                .Include(c => c.Status)
                .ToListAsync();

            // Giả sử pageSize = 10
            int totalPages = (int)Math.Ceiling(comics.Count() / 10.0);
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = 1;

            return View(comics);
        }


        public async Task<IActionResult> LoadComicsPartial(int page = 1, int pageSize = 1)
        {
            int totalComics = await _context.Comics.CountAsync();
            int totalPages = (int)Math.Ceiling(totalComics / (double)pageSize);

            var comics = await _context.Comics
                .Include(c => c.Status)
                .OrderBy(c => c.ComicId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return PartialView("_ComicListPartial", comics);
        }


        // GET: ManageComics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var comic = await _context.Comics.FindAsync(id);
            if (comic == null) return NotFound();

            // Tạo dropdown: Value = StatusId, Text = StatusName, SelectedValue = comic.StatusId
            ViewData["StatusId"] = new SelectList(
                _context.ComicStatuses,
                "StatusId",
                "StatusName",
                comic.StatusId
            );

            return View(comic);
        }

        // POST: ManageComics/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComicId,Title,Description,Author,StatusId,ThumbnailUrl")] Comic comic)
        {
            if (id != comic.ComicId)
            {
                return NotFound();
            }

            // Xóa bỏ các trường không bind hoặc gây lỗi, bao gồm navigation property "Status"
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("Status"); // Loại bỏ lỗi validate của thuộc tính Status

            if (!ModelState.IsValid)
            {
                // Load lại dropdown nếu validation fail
                ViewData["StatusId"] = new SelectList(_context.ComicStatuses, "StatusId", "StatusName", comic.StatusId);
                return View(comic);
            }

            var comicToUpdate = await _context.Comics.FindAsync(id);
            if (comicToUpdate == null)
            {
                return NotFound();
            }

            // Cập nhật các trường cho phép
            comicToUpdate.Title = comic.Title;
            comicToUpdate.Description = comic.Description;
            comicToUpdate.Author = comic.Author;
            comicToUpdate.ThumbnailUrl = comic.ThumbnailUrl;
            comicToUpdate.StatusId = comic.StatusId; // Giá trị từ dropdown
            comicToUpdate.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật truyện thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Comics.Any(e => e.ComicId == comic.ComicId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // GET: ManageComics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var comic = await _context.Comics
                .Include(c => c.Status)
                .FirstOrDefaultAsync(m => m.ComicId == id);
            if (comic == null) return NotFound();

            return View(comic);
        }

        // POST: ManageComics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comic = await _context.Comics.FindAsync(id);
            if (comic != null)
            {
                _context.Comics.Remove(comic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: ManageComics/LoadComicsPartial
      

    }
}
