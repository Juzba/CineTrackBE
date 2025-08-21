namespace CineTrackBE.Models.DTO;

public class SearchParametrsDto
{
    public string? SearchText { get; set; }
    public string? SearchOrder { get; set; }
    public int? GenreId { get; set; }
    public int? SearchByYear { get; set; }

}
