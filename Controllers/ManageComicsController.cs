using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            var nettruyenDbContext = _context.Comics.Include(c => c.Status);
            return View(await nettruyenDbContext.ToListAsync());
        }

        // GET: ManageComics/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comic = await _context.Comics
                .Include(c => c.Status)
                .FirstOrDefaultAsync(m => m.ComicId == id);
            if (comic == null)
            {
                return NotFound();
            }

            return View(comic);
        }

        // GET: ManageComics/Create
        public IActionResult Create()
        {
            ViewData["StatusId"] = new SelectList(_context.ComicStatuses, "StatusId", "StatusId");
            return View();
        }

        // POST: ManageComics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ComicId,Title,Description,Author,StatusId,ThumbnailUrl,CreatedAt,UpdatedAt")] Comic comic)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatusId"] = new SelectList(_context.ComicStatuses, "StatusId", "StatusId", comic.StatusId);
            return View(comic);
        }

        // GET: ManageComics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comic = await _context.Comics.FindAsync(id);
            if (comic == null)
            {
                return NotFound();
            }
            ViewData["StatusId"] = new SelectList(_context.ComicStatuses, "StatusId", "StatusId", comic.StatusId);
            return View(comic);
        }

        // POST: ManageComics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComicId,Title,Description,Author,StatusId,ThumbnailUrl,CreatedAt,UpdatedAt")] Comic comic)
        {
            if (id != comic.ComicId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComicExists(comic.ComicId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatusId"] = new SelectList(_context.ComicStatuses, "StatusId", "StatusId", comic.StatusId);
            return View(comic);
        }

        // GET: ManageComics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comic = await _context.Comics
                .Include(c => c.Status)
                .FirstOrDefaultAsync(m => m.ComicId == id);
            if (comic == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComicExists(int id)
        {
            return _context.Comics.Any(e => e.ComicId == id);
        }
    }
}
