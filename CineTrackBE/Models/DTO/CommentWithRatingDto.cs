using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO;

#nullable disable
public class CommentWithRatingDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "FilmId is required!")]
    public int FilmId { get; set; }

    public string AutorName { get; set; }

    [Required(ErrorMessage = "Text is required!")]
    [MinLength(1, ErrorMessage = "Minimal text lenght is 1!")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rating is required!")]
    [Range(0, 100, ErrorMessage = "Rating musí být mezi 0 a 100!")]
    public int Rating { get; set; }

    public DateTime SendDate { get; set; }
}
