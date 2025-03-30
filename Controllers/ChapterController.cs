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

            // Get comments for this chapter
            var comments = await _context.ChapterComments
                .Where(c => c.ChapterId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new {
                    c.CommentId,
                    c.Content,
                    c.CreatedAt,
                    c.UserId,
                    Username = c.User.Username,
                    UserAvatar = c.User.Avatar
                })
                .Take(20) // Limit to 20 most recent comments
                .ToListAsync();

            ViewBag.Comments = comments;
            ViewBag.CommentCount = comments.Count;
            ViewBag.Images = images;
            return View(chapter);
        }

        [HttpPost]
        public async Task<IActionResult> AddChapterComment(int chapterId, string content)
        {
            try
            {
                // Check if user is logged in
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (userId == 0)
                {
                    return Json(new { success = false, message = "You must be logged in to comment" });
                }

                // Validate content
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Comment cannot be empty" });
                }

                // Create new comment
                var comment = new ChapterComment
                {
                    ChapterId = chapterId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.Now
                };

                _context.ChapterComments.Add(comment);
                await _context.SaveChangesAsync();

                // Get user info for response
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                return Json(new
                {
                    success = true,
                    commentId = comment.CommentId,
                    content = comment.Content,
                    createdAt = string.Format("{0:MMM dd, yyyy HH:mm}", comment.CreatedAt),
                    username = user.Username,
                    userAvatar = user.Avatar != null ? Convert.ToBase64String(user.Avatar) : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
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

        [HttpPost]
        public async Task<IActionResult> DeleteChapterComment(int commentId)
        {
            try
            {
                // Check if user is logged in
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (userId == 0)
                {
                    return Json(new { success = false, message = "You must be logged in to delete comments" });
                }

                // Find the comment
                var comment = await _context.ChapterComments.FirstOrDefaultAsync(c => c.CommentId == commentId);

                // Check if comment exists
                if (comment == null)
                {
                    return Json(new { success = false, message = "Comment not found" });
                }

                // Check if user is the author of the comment
                if (comment.UserId != userId)
                {
                    return Json(new { success = false, message = "You can only delete your own comments" });
                }

                // Remove the comment
                _context.ChapterComments.Remove(comment);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

    }
}
