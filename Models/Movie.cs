using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_mbaizhakyp.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        // To this:
        public string Title { get; set; } = string.Empty;


        [Required]
        [Display(Name = "IMDB Link")]
        [Url]
        public string IMDBHyperlink { get; set; } = string.Empty;

        [Required]
        public string Genre { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Year of Release")]
        public int YearOfRelease { get; set; }

        // This will store the image file's bytes directly in the database
        public byte[]? Poster { get; set; }
        public ICollection<ActorMovie>? ActorMovies { get; set; }
    }
}
