using Microsoft.AspNetCore.Identity;

#nullable disable

namespace CineTrackBE.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SendDate { get; set; }

        // User - Autor //
        public string AutorId { get; set; }
        public User Autor { get; set; }


        // Parrent Comment //
        public int ParrentCommentId { get; set; }
        public Comment ParrentComment { get; set; }


        // Replies //
        public ICollection<Comment> Replies { get; set; } = [];


        // Film //
        public int FilmId { get; set; }
        public Film Film { get; set; }


    }
}
