using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fall2025_Project3_mbaizhakyp.Models
{
    public class ActorMovie
    {
        // Foreign key for the Movie
        public int MovieId { get; set; }
        
        // Navigation property
        public Movie Movie { get; set; } = null!;

        // Foreign key for the Actor
        public int ActorId { get; set; }

        // Navigation property
        public Actor Actor { get; set; } = null!;
    }
}