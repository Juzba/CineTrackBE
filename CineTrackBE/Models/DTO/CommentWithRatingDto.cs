using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO;

#nullable disable
public class CommentWithRatingDto
{
    [Required]
    public int FilmId { get; set; }

    [Required]
    [MinLength(1)]
    public string Text { get; set; } = string.Empty;

    [Required]
    [Range(0, 100, ErrorMessage = "Rating musí být mezi 0 a 100")]
    public int Rating { get; set; }
}
