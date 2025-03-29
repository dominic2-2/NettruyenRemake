using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Helpers;
using NettruyenRemake.Models;

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

        // GET: ManageComics/LoadComicsPartial/
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

        // GET: ManageComics/Create
        public IActionResult Create()
        {
            var categories = _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.AllCategories = categories;
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> FetchMetadata(string site, string url)
        {
            if (site == "mangapark.io")
            {
                try
                {
                    var httpClient = new HttpClient();
                    var html = await httpClient.GetStringAsync(url);


                    // Regex lấy các thể loại
                    var categoryRegex = new Regex(@"<span[^>]*class=""badge[^""]*""[^>]*>(.*?)<\/span>", RegexOptions.IgnoreCase);
                    var categoryMatches = categoryRegex.Matches(html);

                    List<string> categories = new List<string>();
                    foreach (Match catmatch in categoryMatches)
                    {
                        var raw = catmatch.Groups[1].Value.Trim();
                        var decoded = System.Web.HttpUtility.HtmlDecode(raw);
                        if (!string.IsNullOrWhiteSpace(decoded))
                        {
                            categories.Add(decoded);
                        }
                    }


                    var titleRegex = new Regex(@"<h3[^>]*class=""[^""]*font-bold[^""]*""[^>]*>.*?<a[^>]*>(.*?)<\/a>", RegexOptions.IgnoreCase);
                    var match = titleRegex.Match(html);
                    string title = match.Success ? match.Groups[1].Value.Trim() : "Không tìm thấy tiêu đề";
                    title = Regex.Replace(title, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
                    title = System.Web.HttpUtility.HtmlDecode(title);

                    var authorRegex = new Regex(@"<a[^>]*class=""link link-hover link-primary""[^>]*>(.*?)<\/a>", RegexOptions.IgnoreCase);
                    var authorMatch = authorRegex.Match(html);
                    string author = authorMatch.Success ? authorMatch.Groups[1].Value.Trim() : "Không tìm thấy tác giả";
                    author = Regex.Replace(author, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
                    author = System.Web.HttpUtility.HtmlDecode(author);

                    var descriptionRegex = new Regex(@"<div class=""limit-html-p"">(.*?)<\/div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var descriptionMatch = descriptionRegex.Match(html);
                    string description = descriptionMatch.Success ? descriptionMatch.Groups[1].Value.Trim() : "Không tìm thấy mô tả";
                    description = Regex.Replace(description, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
                    description = System.Web.HttpUtility.HtmlDecode(description);

                    var thumbnailRegex = new Regex(@"<img[^>]*src=""([^""]+)""[^>]*>", RegexOptions.IgnoreCase);
                    var thumbnailMatch = thumbnailRegex.Match(html);
                    string thumbnailUrl = thumbnailMatch.Success ? thumbnailMatch.Groups[1].Value : "Không tìm thấy thumbnail";

                    byte[]? imageBytes = null;
                    if (!string.IsNullOrEmpty(thumbnailUrl) && thumbnailUrl != "Không tìm thấy thumbnail")
                    {
                        imageBytes = await ImageHelper.DownloadImageAsync(thumbnailUrl);
                    }

                    return Json(new
                    {
                        title = title,
                        description = description,
                        author = author,
                        thumbnailUrl = thumbnailUrl,
                        thumbnailPreview = imageBytes != null ? Convert.ToBase64String(imageBytes) : "" ,
                        categories = categories
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { error = "Lỗi khi lấy dữ liệu: " + ex.Message });
                }
            }

            return Json(new { error = "Trang chưa hỗ trợ" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveComic([FromForm] Comic comic, [FromForm] string Categories)
        {
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                comic.CreatedAt = DateTime.Now;
                comic.UpdatedAt = DateTime.Now;
                _context.Comics.Add(comic);
                await _context.SaveChangesAsync();

                // Xử lý lưu category
                if (!string.IsNullOrEmpty(Categories))
                {
                    var categoryNames = JsonSerializer.Deserialize<List<string>>(Categories);

                    foreach (var name in categoryNames)
                    {
                        var category = await _context.Categories
                            .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == name.ToLower());

                        if (category == null)
                        {
                            category = new Category { CategoryName = name };
                            _context.Categories.Add(category);
                            await _context.SaveChangesAsync();
                        }

                        // 💥 Thay vì dùng ComicCategory, ta gán trực tiếp:
                        comic.Categories.Add(category);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok();
            }

            return BadRequest();
        }


        // GET: ManageComics/ManageChapters/?comicId=1
        public async Task<IActionResult> ManageChapters(int comicId)
        {
            var comic = _context.Comics
                                 .Where(c => c.ComicId == comicId)
                                 .Select(c => new { c.ComicId, c.Title, c.ThumbnailUrl })
                                 .FirstOrDefault();

            var chapters = _context.Chapters
                                   .Where(c => c.ComicId == comicId)
                                   .OrderByDescending(c => c.ChapterNumber)
                                   .Select(c => new { c.ChapterId, c.Title })
                                   .ToList();

            byte[]? imageBytes = null;
            if (!string.IsNullOrEmpty(comic?.ThumbnailUrl))
            {
                imageBytes = await ImageHelper.DownloadImageAsync(comic.ThumbnailUrl);
            }
            
            ViewBag.Comic = comic;
            ViewBag.Chapters = chapters;
            ViewBag.ThumbnailPreview = imageBytes != null ? Convert.ToBase64String(imageBytes) : "";
            return View();
        }

        // POST: FetchChapterData
        [HttpPost]
        public async Task<JsonResult> FetchChapterData(string site, string url)
        {
            if (site == "mangapark.io")
            {
                try
                {
                    var httpClient = new HttpClient();
                    var html = await httpClient.GetStringAsync(url);

                    // Lấy tên truyện và thumbnail
                    var titleRegex = new Regex(@"<h3[^>]*class=""[^""]*font-bold[^""]*""[^>]*>.*?<a[^>]*>(.*?)<\/a>", RegexOptions.IgnoreCase);
                    var match = titleRegex.Match(html);
                    string title = match.Success ? match.Groups[1].Value.Trim() : "Không tìm thấy tiêu đề";
                    title = Regex.Replace(title, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
                    title = System.Web.HttpUtility.HtmlDecode(title);

                    var thumbnailRegex = new Regex(@"<img[^>]*src=""([^""]+)""[^>]*>", RegexOptions.IgnoreCase);
                    var thumbnailMatch = thumbnailRegex.Match(html);
                    string thumbnailUrl = thumbnailMatch.Success ? thumbnailMatch.Groups[1].Value : "Không tìm thấy thumbnail";

                    byte[]? imageBytes = null;
                    if (!string.IsNullOrEmpty(thumbnailUrl) && thumbnailUrl != "Không tìm thấy thumbnail")
                    {
                        imageBytes = await ImageHelper.DownloadImageAsync(thumbnailUrl);
                    }

                    var chapterRegex = new Regex(@"<a[^>]*class=""link-hover link-primary visited:text-accent""[^>]*>(.*?)<\/a>", RegexOptions.IgnoreCase);
                    var chapterMatches = chapterRegex.Matches(html);

                    var chapters = new List<object>();
                    foreach (Match chapter in chapterMatches)
                    {
                        var linkRegex = new Regex(@"href=""([^""]+)""", RegexOptions.IgnoreCase);
                        var linkMatch = linkRegex.Match(chapter.Value);

                        if (linkMatch.Success)
                        {
                            string chapterLink = "https://mangapark.io" + linkMatch.Groups[1].Value.Trim();

                            string chapterTitle = chapter.Groups[1].Value.Trim();

                            chapters.Add(new
                            {
                                Title = chapterTitle,
                                Link = chapterLink
                            });
                        }
                    }

                    return Json(new
                    {
                        title = title,
                        thumbnailUrl = thumbnailUrl,
                        thumbnailPreview = imageBytes != null ? Convert.ToBase64String(imageBytes) : "",
                        chapters = chapters
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { error = "Lỗi khi lấy dữ liệu chương: " + ex.Message });
                }
            }

            return Json(new { error = "Trang chưa hỗ trợ" });
        }

        // POST: Save Chapter
        [HttpPost]
        public async Task<IActionResult> SaveChapter(int comicId, string chapterTitle, string chapterLink)
        {
            try
            {
                var chapterDataJson = await GetChapterContentFromLink(chapterLink);

                if (chapterDataJson.Contains("\"error\""))
                {
                    return BadRequest(new { message = "Không thể lấy nội dung chương từ đường dẫn cung cấp." });
                }

                var lastChapterNumber = await _context.Chapters
                    .Where(c => c.ComicId == comicId)
                    .MaxAsync(c => (int?)c.ChapterNumber) ?? 0;

                var chapter = new Chapter
                {
                    ComicId = comicId,
                    ChapterNumber = lastChapterNumber + 1,
                    Title = chapterTitle,
                    Content = chapterDataJson,
                    CreatedAt = DateTime.Now
                };

                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Lưu chương thành công.", chapterId = chapter.ChapterId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lưu chương.", detail = ex.Message });
            }
        }

        //GET: ManageComics/GetChapterContentFromLink/?chapterLink=https://mangapark.io/title/233952-en-the-fragrant-flower-blooms-with-dignity/6803178-vol-01-ch-001-rintaro-and-kaoruko
        public async Task<string> GetChapterContentFromLink(string chapterLink)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(chapterLink);

            // Tìm thẻ <script type="qwik/json">...</script>
            var scriptRegex = new Regex(@"<script[^>]*type=""qwik/json""[^>]*>(.*?)<\/script>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var match = scriptRegex.Match(html);

            if (!match.Success)
                return JsonSerializer.Serialize(new { error = "Không tìm thấy script chứa JSON ảnh." });

            string jsonContent = match.Groups[1].Value;

            // Parse JSON
            using var jsonDoc = JsonDocument.Parse(jsonContent);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("objs", out var objsArray))
                return JsonSerializer.Serialize(new { error = "Không tìm thấy mảng ảnh trong JSON." });

            var list = objsArray.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .ToList();

            // Tìm index của "whb"
            int whbIndex = list.FindIndex(s => s == "whb");
            if (whbIndex == -1)
                return JsonSerializer.Serialize(new { error = "Không tìm thấy 'whb' trong mảng." });

            // Duyệt ngược từ whbIndex - 1 trở về, gom các link https cho đến khi gặp chuỗi lạ
            var imageLinks = new List<string>();
            for (int i = whbIndex - 1; i >= 0; i--)
            {
                if (list[i].StartsWith("https://"))
                {
                    imageLinks.Insert(0, list[i]); // Insert đầu để giữ đúng thứ tự ảnh
                }
                else
                {
                    break;
                }
            }

            return JsonSerializer.Serialize(imageLinks);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChapter(int chapterId)
        {
            var chapter = await _context.Chapters
                .Include(c => c.ChapterComments)
                .Include(c => c.ReadingHistories)
                .FirstOrDefaultAsync(c => c.ChapterId == chapterId);

            if (chapter == null)
                return NotFound(new { message = "Không tìm thấy chương." });

            _context.ChapterComments.RemoveRange(chapter.ChapterComments);
            _context.ReadingHistories.RemoveRange(chapter.ReadingHistories);
            _context.Chapters.Remove(chapter);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Chương đã được xóa thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> GetImagePreview(string imageUrl)
        {
            byte[]? imageBytes = null;
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl != "Không tìm thấy thumbnail")
            {
                imageBytes = await ImageHelper.DownloadImageAsync(imageUrl);
            }

            return Json(new
            {
                imagePreview = imageBytes != null ? Convert.ToBase64String(imageBytes) : ""
            });
        }


    }
}
