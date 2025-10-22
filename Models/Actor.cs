using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_mbaizhakyp.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;


        [Required]
        public int Age { get; set; }

        [Required]
        [Display(Name = "IMDB Link")]
        [Url]
        public string IMDBHyperlink { get; set; } = string.Empty;

        // This will store the image file's bytes
        public byte[]? Photo { get; set; }
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}