namespace CineTrackBE.Models.DTO;
#nullable disable


public class CommentDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; }
    public string Text { get; set; }
    public int Rating { get; set; }
    public DateTime SendDate { get; set; }
}
