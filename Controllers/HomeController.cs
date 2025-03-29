using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NettruyenRemake.Models;

namespace NettruyenRemake.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly NettruyenDbContext _context;


        public HomeController(ILogger<HomeController> logger, NettruyenDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 21;

            var top5New = await _context.Comics
                .Include(c => c.Categories)
                .Include(c => c.Status)
                .Include(c => c.Chapters)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(5)
                .ToListAsync();

            var top5Viewed = await _context.ComicStats
                .Include(cs => cs.Comic).ThenInclude(c => c.Categories)
                .Include(cs => cs.Comic).ThenInclude(c => c.Status)
                .Include(cs => cs.Comic).ThenInclude(c => c.Chapters)
                .OrderByDescending(cs => cs.ViewCount)
                .Take(5)
                .Select(cs => cs.Comic)
                .ToListAsync();

            var top5Followed = await _context.ComicStats
                .Include(cs => cs.Comic).ThenInclude(c => c.Status)
                .Include(cs => cs.Comic).ThenInclude(c => c.Chapters)
                .Include(cs => cs.Comic).ThenInclude(c => c.Categories)
                .OrderByDescending(cs => cs.FollowCount)
                .Take(5)
                .Select(cs => cs.Comic)
                .ToListAsync();

            var top5Commented = await _context.ComicStats
                .Include(cs => cs.Comic).ThenInclude(c => c.Chapters)
                .Include(cs => cs.Comic).ThenInclude(c => c.Categories)
                .Include(cs => cs.Comic).ThenInclude(c => c.Status)
                .OrderByDescending(cs => cs.CommentCount)
                .Take(5)
                .Select(cs => cs.Comic)
                .ToListAsync();

            var top5Rated = await _context.ComicStats
                .Include(cs => cs.Comic).ThenInclude(c => c.Status)
                .Include(cs => cs.Comic).ThenInclude(c => c.Categories)
                .Include(cs => cs.Comic).ThenInclude(c => c.Chapters)
                .OrderByDescending(cs => cs.RatingCount)
                .Take(5)
                .Select(cs => cs.Comic)
                .ToListAsync();

            var totalComics = await _context.Chapters
                .Select(c => c.ComicId)
                .Distinct()
                .CountAsync();

            var latestUpdated = await _context.Chapters
                .GroupBy(c => c.ComicId)
                .Select(g => new { ComicId = g.Key, LatestChapterDate = g.Max(c => c.CreatedAt) })
                .OrderByDescending(c => c.LatestChapterDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(_context.Comics,
                      c => c.ComicId,
                      comic => comic.ComicId,
                      (c, comic) => comic)
                .Include(c => c.Categories)
                .Include(c => c.Status)
                .Include(c => c.Chapters)
                .ToListAsync();

            ViewBag.LatestUpdated = latestUpdated;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalComics / (double)pageSize);

            ViewBag.TopViewed = top5Viewed;
            ViewBag.TopFollowed = top5Followed;
            ViewBag.TopCommented = top5Commented;
            ViewBag.TopRated = top5Rated;

            return View(top5New); // model vẫn là top5 new
        }




    }
}
