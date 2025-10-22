using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_mbaizhakyp.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "IMDB Link")]
        [Url]
        public string IMDBHyperlink { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        [Display(Name = "Year of Release")]
        public int YearOfRelease { get; set; }

        // This will store the image file's bytes directly in the database
        public byte[]? Poster { get; set; }
    }
}