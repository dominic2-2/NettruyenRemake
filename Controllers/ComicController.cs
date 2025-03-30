using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using NettruyenRemake.Helpers;

namespace NettruyenRemake.Controllers
{
    public class ComicController : Controller
    {
        private readonly NettruyenDbContext _context;

        public ComicController(NettruyenDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> ListAll(string keyword = "", string sort = "newest", List<int> categoryIds = null, int page = 1, int pageSize = 24)
        {
            var comicsQuery = _context.Comics
                .Include(c => c.Status)
                .Include(c => c.ComicStat)
                .Include(c => c.Categories)
                .Include(c => c.Chapters.OrderByDescending(c => c.ChapterNumber).Take(3))
                .AsSplitQuery()
                .AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                comicsQuery = comicsQuery.Where(c =>
                    c.Title.ToLower().Contains(keyword) ||
                    c.Author.ToLower().Contains(keyword));
            }

            // Lọc theo thể loại
            if (categoryIds != null && categoryIds.Any())
            {
                comicsQuery = comicsQuery
                    .Where(c => categoryIds.All(cid => c.Categories.Select(cat => cat.CategoryId).Contains(cid)));
            }

            // Sắp xếp
            comicsQuery = sort switch
            {
                "views" => comicsQuery.OrderByDescending(c => c.ComicStat.ViewCount),
                "likes" => comicsQuery.OrderByDescending(c => c.ComicStat.FollowCount),
                "recent" => comicsQuery.OrderByDescending(c => c.Chapters.Max(ch => ch.CreatedAt)),
                _ => comicsQuery.OrderByDescending(c => c.UpdatedAt)
            };


            // Tính tổng và áp dụng phân trang
            var totalComics = await comicsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalComics / (double)pageSize);

            var comics = await comicsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Dữ liệu cho dropdown lọc
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sort = sort;
            ViewBag.SelectedCategoryIds = categoryIds ?? new List<int>();
            ViewBag.Keyword = keyword;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Dữ liệu follow & rating
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            ViewBag.FollowedComicIds = userId > 0
                ? await _context.Follows.Where(f => f.UserId == userId).Select(f => f.ComicId).ToListAsync()
                : new List<int>();

            var comicRatings = new Dictionary<int, double>();
            var comicRatingCounts = new Dictionary<int, int>();

            foreach (var comic in comics)
            {
                var avgRating = await GetAverageRating(comic.ComicId);
                comicRatings[comic.ComicId] = Math.Round(avgRating, 1);

                var ratingCount = await _context.Ratings
                    .Where(r => r.ComicId == comic.ComicId)
                    .CountAsync();
                comicRatingCounts[comic.ComicId] = ratingCount;
            }

            ViewBag.ComicRatings = comicRatings;
            ViewBag.ComicRatingCounts = comicRatingCounts;

            return View(comics);
        }



        // Get comic details
        public async Task<IActionResult> Details(int id)
        {
            var comic = await _context.Comics
                .Include(c => c.Status)
                .Include(c => c.ComicStat)
                .Include(c => c.Categories)
                .Include(c => c.Chapters.OrderByDescending(c => c.ChapterNumber))
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.ComicId == id);

            if (comic == null)
            {
                return NotFound();
            }

            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Check if the user is following this comic
            bool isFollowing = false;
            if (userId > 0)
            {
                isFollowing = await _context.Follows
                    .AnyAsync(f => f.UserId == userId && f.ComicId == id);

                // Get user's rating for this comic
                var userRating = await GetUserRating(userId, id);
                ViewBag.UserRating = userRating;
            }

            // Get average rating
            var averageRating = await GetAverageRating(id);
            ViewBag.AverageRating = Math.Round(averageRating, 1);

            // Get rating count
            var ratingCount = await _context.Ratings
                .Where(r => r.ComicId == id)
                .CountAsync();
            ViewBag.RatingCount = ratingCount;

            ViewBag.IsFollowing = isFollowing;

            // Update view count
            if (comic.ComicStat != null)
            {
                comic.ComicStat.ViewCount = (comic.ComicStat.ViewCount ?? 0) + 1;
                comic.ComicStat.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return View(comic);
        }

        private async Task<double> GetAverageRating(int comicId)
        {
            return await _context.Ratings
                .Where(r => r.ComicId == comicId)
                .AverageAsync(r => (double?)r.RatingValue) ?? 0;
        }

        private async Task<int> GetUserRating(int userId, int comicId)
        {
            return await _context.Ratings
                .Where(r => r.UserId == userId && r.ComicId == comicId)
                .Select(r => r.RatingValue ?? 0)
                .FirstOrDefaultAsync();
        }

        // NEW: MyFollowings action method moved from FollowController
        public async Task<IActionResult> MyFollowings()
        {
            // Get current user ID from session
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                return RedirectToAction("Login", "Authen");
            }

            // Get all comics that the user is following, ordered by follow date (newest first)
            var followedComics = await _context.Comics
                .Include(c => c.Status)
                .Include(c => c.ComicStat)
                .Include(c => c.Chapters)
                .Include(c => c.Follows.Where(f => f.UserId == userId)) // Include the follows for ordering
                .Where(c => c.Follows.Any(f => f.UserId == userId))
                .OrderByDescending(c => c.Follows.FirstOrDefault(f => f.UserId == userId).FollowedAt) // Order by follow date
                .AsSplitQuery()
                .ToListAsync();

            // Calculate average ratings for each comic
            var comicRatings = new Dictionary<int, double>();
            var comicRatingCounts = new Dictionary<int, int>();

            foreach (var comic in followedComics)
            {
                var avgRating = await GetAverageRating(comic.ComicId);
                comicRatings[comic.ComicId] = Math.Round(avgRating, 1);

                var ratingCount = await _context.Ratings
                    .Where(r => r.ComicId == comic.ComicId)
                    .CountAsync();
                comicRatingCounts[comic.ComicId] = ratingCount;
            }

            ViewBag.ComicRatings = comicRatings;
            ViewBag.ComicRatingCounts = comicRatingCounts;

            return View(followedComics);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleFollow(int comicId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Check if the user is already following this comic
                var existingFollow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ComicId == comicId);

                if (existingFollow != null)
                {
                    // If already following, unfollow
                    _context.Follows.Remove(existingFollow);

                    // Update stats
                    var comicStat = await _context.ComicStats.FirstOrDefaultAsync(cs => cs.ComicId == comicId);
                    if (comicStat != null && comicStat.FollowCount > 0)
                    {
                        comicStat.FollowCount--;
                        comicStat.LastUpdated = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, following = false });
                }
                else
                {
                    // If not following, add follow
                    var follow = new Follow
                    {
                        UserId = userId,
                        ComicId = comicId,
                        FollowedAt = DateTime.Now
                    };

                    _context.Follows.Add(follow);

                    // Update stats
                    var comicStat = await _context.ComicStats.FirstOrDefaultAsync(cs => cs.ComicId == comicId);
                    if (comicStat != null)
                    {
                        comicStat.FollowCount = (comicStat.FollowCount ?? 0) + 1;
                        comicStat.LastUpdated = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, following = true });
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in ToggleFollow: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RateComic(int comicId, int rating)
        {
            try
            {
                // Check if rating is valid (between 1 and 5)
                if (rating < 1 || rating > 5)
                {
                    return Json(new { success = false, message = "Rating must be between 1 and 5" });
                }

                // Get current user ID from session
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Check if user has already rated this comic
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ComicId == comicId);

                if (existingRating != null)
                {
                    // Update existing rating
                    existingRating.RatingValue = rating;
                    existingRating.CreatedAt = DateTime.Now;
                }
                else
                {
                    // Add new rating
                    var newRating = new Rating
                    {
                        UserId = userId,
                        ComicId = comicId,
                        RatingValue = rating,
                        CreatedAt = DateTime.Now
                    };

                    _context.Ratings.Add(newRating);
                }

                await _context.SaveChangesAsync();

                // Calculate the new average rating
                var avgRating = await _context.Ratings
                    .Where(r => r.ComicId == comicId)
                    .AverageAsync(r => r.RatingValue) ?? 0;

                var ratingCount = await _context.Ratings
                    .Where(r => r.ComicId == comicId)
                    .CountAsync();

                return Json(new
                {
                    success = true,
                    averageRating = Math.Round(avgRating, 1),
                    ratingCount = ratingCount
                });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in RateComic: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRating(int comicId)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var userRating = await _context.Ratings
                .Where(r => r.UserId == userId && r.ComicId == comicId)
                .Select(r => r.RatingValue)
                .FirstOrDefaultAsync() ?? 0;

            return Json(new { success = true, rating = userRating });
        }

        
        [HttpGet]
        [Route("comic/image")]
        public async Task<IActionResult> GetComicImage(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return NotFound();
            }

            try
            {
               
                url = Uri.UnescapeDataString(url);
                
              
                var imageBytes = await ImageHelper.DownloadImageAsync(url);
                
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return NotFound();
                }
                
                // Determine content type based on URL extension
                string contentType = "image/jpeg"; // Default
                if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";
                else if (url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/gif";
                else if (url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/webp";
                
                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error proxying image: {ex.Message}");
                return NotFound();
            }
        }

        [HttpGet("/proxy-image")]
        public async Task<IActionResult> ProxyImage(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(url);
                return File(imageBytes, "image/jpeg");
            }
            catch
            {
                var fallbackPath = Path.Combine("wwwroot", "images", "default-thumbnail.jpg");
                var fallbackBytes = await System.IO.File.ReadAllBytesAsync(fallbackPath);
                return File(fallbackBytes, "image/jpeg");
            }
        }
    }
}