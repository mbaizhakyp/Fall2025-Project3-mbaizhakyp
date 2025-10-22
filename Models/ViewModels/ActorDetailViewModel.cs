using System.Collections.Generic;

namespace Fall2025_Project3_mbaizhakyp.Models.ViewModels
{
    public class ActorDetailViewModel
    {
        public Actor Actor { get; set; } = null!;

        // List of movies this actor is in (for the bonus)
        public List<Movie> Movies { get; set; } = new List<Movie>();

        // AI-generated tweets
        public List<TweetSentimentViewModel> Tweets { get; set; } = new List<TweetSentimentViewModel>();

        // Overall sentiment
        public string OverallSentiment { get; set; } = string.Empty;
    }
}