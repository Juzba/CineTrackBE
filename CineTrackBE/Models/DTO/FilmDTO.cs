#nullable disable

using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO;



public class FilmDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required!")]
    public string Name { get; set; }
    public string Description { get; set; }
    [Required(ErrorMessage = "Director is required")]
    public string Director { get; set; }
    public string ImageFileName { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsMyFavorite { get; set; }
    public double AvgRating { get; set; }

    [Required(ErrorMessage = "Genres is required!")]
    [MinLength(1, ErrorMessage ="Minimum Genre is one!")]
    [MaxLength(3, ErrorMessage = "Maximum Genres are 3!")]
    public List<GenreDto> Genres { get; set; }
}
