using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;

namespace NettruyenRemake.Controllers
{
    public class DashboardController : Controller
    {
        private readonly NettruyenDbContext _context;

        public DashboardController(NettruyenDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                UserRegistrations = await GetUserRegistrations(),
                RoleDistributions = await GetRoleDistribution(),
                TopViewedComics = await GetTopViewedComics(),
                TopFollowedComics = await GetTopFollowedComics(),
                ComicStatusDistribution = await GetComicStatusDistribution(),
                CategoryDistribution = await GetCategoryDistribution(),
                RatingActivity = await GetRatingActivity(),
                CommentActivity = await GetCommentActivity(),
                MostCommentedComics = await GetMostCommentedComics(),
                ReadingActivity = await GetReadingActivity(),
                ReadingPeakHours = await GetReadingPeakHours()
            };

            return View(model);
        }

        private async Task<List<UserRegistrationData>> GetUserRegistrations()
        {
            return await _context.Users
                .Where(u => u.CreatedAt != null)
                .GroupBy(u => u.CreatedAt.Value.Date)
                .Select(g => new UserRegistrationData
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        private async Task<List<RoleDistribution>> GetRoleDistribution()
        {
            return await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => u.Role.RoleName)
                .Select(g => new RoleDistribution
                {
                    RoleName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
        }

        private async Task<List<ComicStat>> GetTopViewedComics(int count = 5)
        {
            return await _context.ComicStats
                .Include(cs => cs.Comic)
                .OrderByDescending(cs => cs.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<StatusDistribution>> GetComicStatusDistribution()
        {
            return await _context.Comics
                .Include(c => c.Status)
                .GroupBy(c => c.Status.StatusName)
                .Select(g => new StatusDistribution
                {
                    StatusName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
        }

        private async Task<List<CategoryDistribution>> GetCategoryDistribution()
        {
            return await _context.Categories
                .Select(c => new CategoryDistribution
                {
                    CategoryName = c.CategoryName,
                    Count = c.Comics.Count
                })
                .ToListAsync();
        }

        private async Task<List<ReadingData>> GetReadingActivity()
        {
            return await _context.ReadingHistories
                .Where(rh => rh.LastReadAt != null)
                .GroupBy(rh => rh.LastReadAt.Value.Date)
                .Select(g => new ReadingData
                {
                    Date = g.Key,
                    ReadCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }
        // Example for comments
        private async Task<List<InteractionData>> GetCommentActivity()
        {
            // Get comic comments
            var comicComments = await _context.ComicComments
                .Where(cc => cc.CreatedAt != null)
                .GroupBy(cc => cc.CreatedAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get chapter comments
            var chapterComments = await _context.ChapterComments
                .Where(cc => cc.CreatedAt != null)
                .GroupBy(cc => cc.CreatedAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Combine and aggregate results
            var combinedComments = comicComments
                .Concat(chapterComments)
                .GroupBy(c => c.Date)
                .Select(g => new InteractionData
                {
                    Date = g.Key,
                    Count = g.Sum(x => x.Count)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return combinedComments;
        }
        private async Task<List<ComicStat>> GetTopFollowedComics(int count = 5)
        {
            return await _context.ComicStats
                .Include(cs => cs.Comic)
                .OrderByDescending(cs => cs.FollowCount)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<InteractionData>> GetRatingActivity()
        {
            return await _context.Ratings
                .Where(r => r.CreatedAt != null)
                .GroupBy(r => r.CreatedAt.Value.Date)
                .Select(g => new InteractionData
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        private async Task<List<MostCommentedComic>> GetMostCommentedComics(int count = 5)
        {
            return await _context.Comics
                .Select(c => new MostCommentedComic
                {
                    Title = c.Title,
                    TotalComments = _context.ComicComments.Count(cc => cc.ComicId == c.ComicId) +
                                   _context.ChapterComments.Count(ch => ch.Chapter.ComicId == c.ComicId)
                })
                .OrderByDescending(c => c.TotalComments)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<PeakHourData>> GetReadingPeakHours()
        {
            return await _context.ReadingHistories
                .Where(rh => rh.LastReadAt != null)
                .GroupBy(rh => rh.LastReadAt.Value.Hour)
                .Select(g => new PeakHourData
                {
                    Hour = g.Key,
                    ReadCount = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToListAsync();
        }
    }
}

