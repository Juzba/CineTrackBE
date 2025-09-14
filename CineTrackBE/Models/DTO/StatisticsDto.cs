using Humanizer;

namespace CineTrackBE.Models.DTO;

public class StatisticsDto
{
    public Overview Overview { get; set; } = new();
    public TopMovies TopMovies { get; set; } = new();
    public UserActivity UserActivity { get; set; } = new();

}

public class Overview
{
    public int TotalMovies { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRatings { get; set; }
    public double AverageRating { get; set; }
    public int TotalComments { get; set; }

}

public class TopMovies
{
    public List<FilmDto> BestRated { get; set; } = new();
    public List<FilmDto> MostPopular { get; set; } = new();
    public List<FilmDto> Newest { get; set; } = new();
}


public class UserActivity
{
    public double AverageCommentsPerUser { get; set; }
    public List<UserDto> MostActiveUsers { get; set; } = new();

}