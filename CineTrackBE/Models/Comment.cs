using Microsoft.AspNetCore.Identity;

namespace CineTrackBE.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime SendDate { get; set; }

        // User - Autor //
        public string AutorId { get; set; } = null!;
        public IdentityUser Autor { get; set; } = null!;


        // Parrent Comment //
        public int ParrentCommentId { get; set; }
        public Comment? ParrentComment { get; set; }


        // Replies //
        public ICollection<Comment> Replies { get; set; } = [];


        // Film //
        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;


    }
}
