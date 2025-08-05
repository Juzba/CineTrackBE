namespace CineTrackBE.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;


        // Film Genre //
        public ICollection<FilmGenre> FilmGenres { get; set; } = [];

    }
}
