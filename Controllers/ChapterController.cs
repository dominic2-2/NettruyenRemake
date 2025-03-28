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
                    existingHistory.LastReadAt = DateTime.Now;
                }
                else
                {
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

            // 👉 Giải mã JSON -> List<string>
            List<string> images = new();
            if (!string.IsNullOrEmpty(chapter.Content))
            {
                images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(chapter.Content) ?? new List<string>();
            }
            // Lấy toàn bộ danh sách chương thuộc truyện này
            var allChapters = await _context.Chapters
                .Where(c => c.ComicId == chapter.ComicId)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            var currentIndex = allChapters.FindIndex(c => c.ChapterId == id);

            Chapter? previousChapter = currentIndex > 0 ? allChapters[currentIndex - 1] : null;
            Chapter? nextChapter = currentIndex < allChapters.Count - 1 ? allChapters[currentIndex + 1] : null;

            ViewBag.PreviousChapter = previousChapter;
            ViewBag.NextChapter = nextChapter;
            ViewBag.ChapterList = allChapters;

            ViewBag.Images = images;
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
