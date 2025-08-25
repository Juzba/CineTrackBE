namespace CineTrackBE.Models.DTO;

public class UserProfilDataDto
{
    public ICollection<FavoriteFilms> FavoriteFilms { get; set; } = [];
    public ICollection<LatestComment> LatestComments { get; set; } = [];

    public int TotalComments { get; set; }
    public int AvgRating { get; set; }
    public int TopRating { get; set; }

    public string LastFavoriteFilmTitle { get; set; } = string.Empty;
    public int FavoriteFilmsCount { get; set; }
}

public class FavoriteFilms
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}


public class LatestComment
{
    public int Id { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public DateTime CommentDate { get; set; }
    public int Rating { get; set; }
}