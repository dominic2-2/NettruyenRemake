using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;

namespace NettruyenRemake.Controllers
{
    public class ChapterController : Controller
    {
        private readonly NettruyenDbContext _context;

        public ChapterController()
        {
            _context = new NettruyenDbContext();
        }

        public async Task<IActionResult> Read(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Comic)
                .FirstOrDefaultAsync(c => c.ChapterId == id);

            if (chapter == null)
                return NotFound();

            // 👉 Lấy UserId từ Session
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                var existingHistory = await _context.ReadingHistories.FirstOrDefaultAsync(h =>
                    h.UserId == userId.Value &&
                    h.ComicId == chapter.ComicId &&
                    h.ChapterId == chapter.ChapterId);

                if (existingHistory != null)
                {
                    // Cập nhật thời gian đọc mới nhất
                    existingHistory.LastReadAt = DateTime.Now;
                }
                else
                {
                    // Tạo mới bản ghi lịch sử đọc
                    var history = new ReadingHistory
                    {
                        UserId = userId.Value,
                        ComicId = chapter.ComicId,
                        ChapterId = chapter.ChapterId,
                        LastReadAt = DateTime.Now
                    };

                    _context.ReadingHistories.Add(history);
                }

                await _context.SaveChangesAsync();
            }

            return View(chapter);
        }
        public async Task<IActionResult> History()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Authen");

            var userHistories = await _context.ReadingHistories
                .Where(h => h.UserId == userId)
                .Include(h => h.Comic)
                .Include(h => h.Chapter)
                .ToListAsync();

            // Với mỗi truyện, lấy chương có LastReadAt mới nhất
            var latestPerComic = userHistories
                .GroupBy(h => h.ComicId)
                .Select(g => g.OrderByDescending(x => x.LastReadAt).First())
                .OrderByDescending(h => h.LastReadAt)
                .ToList();

            return View("ReadingHistory", latestPerComic);
        }



    }
}
