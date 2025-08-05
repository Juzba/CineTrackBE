using System.ComponentModel.DataAnnotations.Schema;

namespace CineTrackBE.Models;

public class Film
{

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Director { get; set; } = null!;
    public int Year { get; set; }


    // Film Genre //
    public ICollection<FilmGenre> FilmGenres { get; set; } = [];


    // Ratings //
    public ICollection<Rating> Ratings { get; set; } = [];


    // Comments //
    public ICollection<Comment> Comments { get; set; } = [];


    [NotMapped]
    public double AvgRating => Ratings.Count != 0 ? Ratings.Average(p => p.UserRating) : 0;

}
