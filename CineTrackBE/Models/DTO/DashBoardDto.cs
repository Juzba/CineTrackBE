namespace CineTrackBE.Models.DTO
{
# nullable disable
    public class DashBoardDto
    {
        public List<FilmDto> LatestFilms { get; set; }
        public List<FilmDto> FavoriteFilms { get; set; }
    }
}
