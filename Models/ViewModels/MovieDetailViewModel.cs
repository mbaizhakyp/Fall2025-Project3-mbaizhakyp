using System.Collections.Generic;

namespace Fall2025_Project3_mbaizhakyp.Models.ViewModels
{
    public class MovieDetailViewModel
    {
        // The movie itself
        public Movie Movie { get; set; } = null!;

        // List of actors in this movie (for the bonus)
        public List<Actor> Actors { get; set; } = new List<Actor>();

        // AI-generated reviews
        public List<ReviewSentimentViewModel> Reviews { get; set; } = new List<ReviewSentimentViewModel>();

        // Overall sentiment
        public string OverallSentiment { get; set; } = string.Empty;
    }
}