using CineTrackBE.Models.Attributes;
using CineTrackBE.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.ViewModel;
#nullable disable
public class FilmViewModel
{
    public Film Film { get; set; }

    public List<Genre> AllGenres { get; set; } = [];


    [Required(ErrorMessage = "Musí být vybrán aspoň jeden žánr!")]
    [MaxElements(3, ErrorMessage ="Můžou být maximálně 3 žánry!")]
    public List<int> SelectedGenresId { get; set; } = [];


}
