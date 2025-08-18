using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CineTrackBE.Models.Entities;

public class Film
{

    public int Id { get; set; }

    [Required(ErrorMessage = "Chybí název!")]
    public string Name { get; set; }
    public string Description { get; set; }
    public string Director { get; set; }
    public string ImageFileName { get; set; }
    public DateTime ReleaseDate { get; set; }


    // Film Genre //
    public ICollection<FilmGenre> FilmGenres { get; set; } = [];


    // Ratings //
    public ICollection<Rating> Ratings { get; set; } = [];


    // Comments //
    public ICollection<Comment> Comments { get; set; } = [];


    [NotMapped]
    public double AvgRating => Ratings.Count != 0 ? Ratings.Average(p => p.UserRating) : 0;

}
