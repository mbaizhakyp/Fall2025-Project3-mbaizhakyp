using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_mbaizhakyp.Data;
using Fall2025_Project3_mbaizhakyp.Models;
using Fall2025_Project3_mbaizhakyp.Services;
using Fall2025_Project3_mbaizhakyp.Models.ViewModels;
using VaderSharp2;

namespace Fall2025_Project3_mbaizhakyp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAIService; // <-- ADDED

        // --- CONSTRUCTOR UPDATED ---
        public MoviesController(ApplicationDbContext context, OpenAIService openAIService)
        {
            _context = context;
            _openAIService = openAIService; // <-- ADDED
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // --- DETAILS (GET) METHOD FULLY REPLACED ---
        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Get the movie and its related actors (for the bonus)
            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            // 2. Create the ViewModel
            var viewModel = new MovieDetailViewModel
            {
                Movie = movie,
                Actors = movie.ActorMovies?.Select(am => am.Actor).ToList() ?? new List<Actor>()
            };

            // 3. Call AI for 10 reviews
            string prompt = $"Generate 10 brief, unique reviews for the movie '{movie.Title}'. Each review should be on a new line and start with a number (e.g., '1. ...').";
            string aiResponse = await _openAIService.GetChatCompletionAsync(prompt);

            // 4. Parse response and run Sentiment Analysis
            var analyzer = new SentimentIntensityAnalyzer();
            var reviews = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            double totalSentiment = 0;
            int reviewCount = 0;

            foreach (var reviewText in reviews)
            {
                // Clean up the text (e.g., remove "1. ")
                var cleanText = reviewText.Length > 3 ? reviewText.Substring(3) : reviewText;
                
                var sentiment = analyzer.PolarityScores(cleanText);
                
                viewModel.Reviews.Add(new ReviewSentimentViewModel
                {
                    ReviewText = cleanText,
                    SentimentScore = sentiment.Compound
                });

                totalSentiment += sentiment.Compound;
                reviewCount++;
            }

            // 5. Calculate and format average sentiment
            if (reviewCount > 0)
            {
                double avgSentiment = totalSentiment / reviewCount;
                if (avgSentiment > 0.05)
                    viewModel.OverallSentiment = $"Positive (Score: {avgSentiment:F2})";
                else if (avgSentiment < -0.05)
                    viewModel.OverallSentiment = $"Negative (Score: {avgSentiment:F2})";
                else
                    viewModel.OverallSentiment = $"Neutral (Score: {avgSentiment:F2})";
            }
            else
            {
                viewModel.OverallSentiment = "Could not be determined.";
            }
            
            return View(viewModel);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDBHyperlink,Genre,YearOfRelease")] Movie movie, IFormFile? posterFile)
        {
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? posterFile)
        {   
            var movieToUpdate = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            
            if (movieToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(movieToUpdate, "",
                m => m.Title, m => m.IMDBHyperlink, m => m.Genre, m => m.YearOfRelease))
            {
                if (posterFile != null && posterFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await posterFile.CopyToAsync(memoryStream);
                        movieToUpdate.Poster = memoryStream.ToArray();
                        _context.Entry(movieToUpdate).Property(x => x.Poster).IsModified = true;
                    }
                }

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