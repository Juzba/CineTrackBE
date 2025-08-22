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
        public ApplicationUser Autor { get; set; }

        // Film //
        public int FilmId { get; set; }

        // Rating //
        public int RatingId { get; set; }
        public Rating Rating { get; set; }
       


    }
}
