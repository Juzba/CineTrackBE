namespace CineTrackBE.Models
{
    public class FilmGenre
    {
        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;



        // Genre //
        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;




    }
}
