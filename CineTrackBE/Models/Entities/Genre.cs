using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.Entities
{
    public class Genre
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Chybí název!")]
        public string Name { get; set; } = null!;


        // Film Genre //
        public ICollection<FilmGenre> FilmGenres { get; set; } = [];

    }
}
