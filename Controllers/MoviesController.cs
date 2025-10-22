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
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // UPDATED: Added IFormFile? posterFile, removed Poster from [Bind]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDBHyperlink,Genre,YearOfRelease")] Movie movie, IFormFile? posterFile)
        {
            // UPDATED: Added this block to handle the file upload
            if (posterFile != null && posterFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await posterFile.CopyToAsync(memoryStream);
                    movie.Poster = memoryStream.ToArray();
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // UPDATED: This entire method is replaced to fix the 405 error
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? posterFile)
        {   
            
            // 1. Fetch the existing movie from the database
            var movieToUpdate = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);

            
            if (movieToUpdate == null)
            {
                return NotFound();
            }

            // 2. Try to update its properties from the form
            if (await TryUpdateModelAsync(movieToUpdate, "",
                m => m.Title, m => m.IMDBHyperlink, m => m.Genre, m => m.YearOfRelease))
            {
                // 3. Handle the file upload
                if (posterFile != null && posterFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await posterFile.CopyToAsync(memoryStream);
                        movieToUpdate.Poster = memoryStream.ToArray();
                        
                        // --- THIS IS THE UPDATED LINE ---
                        // Force EF to recognize the Poster has changed
                        _context.Entry(movieToUpdate).Property(x => x.Poster).IsModified = true;
                    }
                }

                // 4. Save changes
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movieToUpdate.Id))
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
            
            // If TryUpdateModelAsync fails, return to the Edit view
            return View(movieToUpdate);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}