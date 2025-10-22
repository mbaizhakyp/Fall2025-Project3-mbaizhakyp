using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_mbaizhakyp.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        [Display(Name = "IMDB Link")]
        [Url]
        public string IMDBHyperlink { get; set; }

        // This will store the image file's bytes
        public byte[]? Photo { get; set; }
    }
}