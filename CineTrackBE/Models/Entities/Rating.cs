namespace CineTrackBE.Models.Entities;
#nullable disable


public class Rating
{
    public int Id { get; set; }
    public int UserRating { get; set; }


    // Comment
    public int CommentId { get; set; }
    public Comment Comment { get; set; }

}
