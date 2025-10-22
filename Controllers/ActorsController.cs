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
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAIService; // <-- ADDED

        // UPDATED CONSTRUCTOR
        public ActorsController(ApplicationDbContext context, OpenAIService openAIService)
        {
            _context = context;
            _openAIService = openAIService; // <-- ADDED
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        // --- THIS ENTIRE METHOD IS REPLACED ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Get the actor and their related movies (for the bonus)
            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            // 2. Create the ViewModel
            var viewModel = new ActorDetailViewModel
            {
                Actor = actor,
                Movies = actor.ActorMovies?.Select(am => am.Movie).ToList() ?? new List<Movie>()
            };

            // 3. Call AI for 20 tweets
            string prompt = $"Generate 20 fake, brief, unique tweets about the actor '{actor.Name}'. Each tweet should be on a new line and start with a number (e.g., '1. ...').";
            string aiResponse = await _openAIService.GetChatCompletionAsync(prompt);

            // 4. Parse response and run Sentiment Analysis
            var analyzer = new SentimentIntensityAnalyzer();
            var tweets = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            double totalSentiment = 0;
            int tweetCount = 0;

            foreach (var tweetText in tweets)
            {
                // Clean up the text (e.g., remove "1. ")
                var cleanText = tweetText.Length > 3 ? tweetText.Substring(3) : tweetText;
                
                var sentiment = analyzer.PolarityScores(cleanText);
                
                viewModel.Tweets.Add(new TweetSentimentViewModel
                {
                    TweetText = cleanText,
                    SentimentScore = sentiment.Compound
                });

                totalSentiment += sentiment.Compound;
                tweetCount++;
            }

            // 5. Calculate and format average sentiment
            if (tweetCount > 0)
            {
                double avgSentiment = totalSentiment / tweetCount;
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

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBHyperlink")] Actor actor, IFormFile? photoFile)
        {
            if (photoFile != null && photoFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await photoFile.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }
            }
            
            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? photoFile)
        {
            var actorToUpdate = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);

            if (actorToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(actorToUpdate, "",
                a => a.Name, a => a.Gender, a => a.Age, a => a.IMDBHyperlink))
            {
                if (photoFile != null && photoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await photoFile.CopyToAsync(memoryStream);
                        actorToUpdate.Photo = memoryStream.ToArray();
                        
                        // --- THIS IS THE UPDATED LINE ---
                        // Force EF to recognize the Photo has changed
                        _context.Entry(actorToUpdate).Property(x => x.Photo).IsModified = true;
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actorToUpdate.Id))
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
            
            return View(actorToUpdate);
        }


        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }
    }
}