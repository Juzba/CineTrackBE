using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO
{
    public class GenreDto
    {
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; } = null!;

    }
}
