#nullable disable

namespace CineTrackBE.Models.DTO;



public class FilmDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Director { get; set; }
    public string ImageFileName { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsMyFavorite { get; set; }
    public double AvgRating { get; set; }
    public List<GenreDto> Genres { get; set; }
}
