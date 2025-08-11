using CineTrackBE.Models.Entities;

namespace CineTrackBE.Models.ViewModel;
#nullable disable
public class FilmViewModel
{
    public Film Film { get; set; }

    public List<Genre> AllGenres { get; set; } = [];
    public List<Genre> SelectedGenres { get; set; } = [];


}
