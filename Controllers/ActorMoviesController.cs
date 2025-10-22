using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_mbaizhakyp.Data;
using Fall2025_Project3_mbaizhakyp.Models;

namespace Fall2025_Project3_mbaizhakyp.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActorMovies
        public async Task<IActionResult> Index()
        {
            // This is correct
            var applicationDbContext = _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ActorMovies/Details?movieId=5&actorId=10
        public async Task<IActionResult> Details(int? movieId, int? actorId) // <-- UPDATED
        {
            if (movieId == null || actorId == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.MovieId == movieId && m.ActorId == actorId); // <-- UPDATED
            
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        // GET: ActorMovies/Create
        public IActionResult Create()
        {
            // This is correct
            ViewData["ActorId"] = new SelectList(_context.Actors.OrderBy(a => a.Name), "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title");
            return View();
        }

        // POST: ActorMovies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,ActorId")] ActorMovie actorMovie)
        {
            ModelState.Remove("Movie");
            ModelState.Remove("Actor");
            if (ModelState.IsValid)
            {
                // This duplicate check is correct
                var relationshipExists = await _context.ActorMovies
                    .AnyAsync(am => am.ActorId == actorMovie.ActorId && am.MovieId == actorMovie.MovieId);

                if (relationshipExists)
                {
                    ModelState.AddModelError(string.Empty, "This actor is already linked to this movie.");
                    
                    // This repopulation is correct
                    ViewData["ActorId"] = new SelectList(_context.Actors.OrderBy(a => a.Name), "Id", "Name", actorMovie.ActorId);
                    ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", actorMovie.MovieId);
                    return View(actorMovie);
                }

                _context.Add(actorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // UPDATED: Repopulate dropdowns correctly on model state failure
            ViewData["ActorId"] = new SelectList(_context.Actors.OrderBy(a => a.Name), "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", actorMovie.MovieId);
            return View(actorMovie);
        }

        // GET: ActorMovies/Edit?movieId=5&actorId=10
        public async Task<IActionResult> Edit(int? movieId, int? actorId) // <-- UPDATED
        {
            if (movieId == null || actorId == null)
            {
                return NotFound();
            }

            // UPDATED: Find by composite key
            var actorMovie = await _context.ActorMovies.FindAsync(movieId, actorId);
            if (actorMovie == null)
            {
                return NotFound();
            }
            
            // UPDATED: Populate dropdowns with names
            ViewData["ActorId"] = new SelectList(_context.Actors.OrderBy(a => a.Name), "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", actorMovie.MovieId);
            return View(actorMovie);
        }

        // POST: ActorMovies/Edit?movieId=5&actorId=10
        [HttpPost]
        [ValidateAntiForgeryToken]
        // UPDATED: Changed 'int id' to 'int movieId, int actorId'
        public async Task<IActionResult> Edit(int movieId, int actorId, [Bind("MovieId,ActorId")] ActorMovie actorMovie)
        {
            // UPDATED: Validate both keys
            if (movieId != actorMovie.MovieId || actorId != actorMovie.ActorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actorMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorMovieExists(actorMovie.MovieId, actorMovie.ActorId)) // <-- UPDATED
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
            
            // UPDATED: Repopulate dropdowns correctly
            ViewData["ActorId"] = new SelectList(_context.Actors.OrderBy(a => a.Name), "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", actorMovie.MovieId);
            return View(actorMovie);
        }

        // GET: ActorMovies/Delete?movieId=5&actorId=10
        public async Task<IActionResult> Delete(int? movieId, int? actorId) // <-- UPDATED
        {
            if (movieId == null || actorId == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.MovieId == movieId && m.ActorId == actorId); // <-- UPDATED
            
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        // POST: ActorMovies/Delete?movieId=5&actorId=10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int movieId, int actorId) // <-- UPDATED
        {
            // UPDATED: Find by composite key
            var actorMovie = await _context.ActorMovies.FindAsync(movieId, actorId);
            if (actorMovie != null)
            {
                _context.ActorMovies.Remove(actorMovie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATED: Check for existence using both keys
        private bool ActorMovieExists(int movieId, int actorId)
        {
            return _context.ActorMovies.Any(e => e.MovieId == movieId && e.ActorId == actorId);
        }
    }
}