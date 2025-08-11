namespace CineTrackBE.Models.Entities;

#nullable disable
public class FilmGenre
{
    public int Id { get; set; }


    // Film //
    public int FilmId { get; set; }
    public Film Film { get; set; }



    // Genre //
    public int GenreId { get; set; }
    public Genre Genre { get; set; }




}
