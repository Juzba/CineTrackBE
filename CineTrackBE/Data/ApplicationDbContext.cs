using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole, string>
    {

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<FilmGenre> FilmGenres { get; set; }
        public DbSet<Genre> Genre { get; set; }
        public DbSet<Rating> Ratings { get; set; }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Seed initial data //
            SeedData.Seed(builder);

            builder.Entity<FilmGenre>()
                .HasKey(p => new { p.FilmId, p.GenreId });

            builder.Entity<FilmGenre>()
                .HasOne(p => p.Film)
                .WithMany(p => p.FilmGenres)
                .HasForeignKey(p => p.FilmId);


            builder.Entity<FilmGenre>()
                .HasOne(p => p.Genre)
                .WithMany(p => p.FilmGenres)
                .HasForeignKey(p => p.GenreId);

            builder.Entity<Comment>()
                .HasOne(p => p.ParrentComment)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParrentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
                .HasOne(p => p.Film)
                .WithMany(p => p.Comments)
                .HasForeignKey(p => p.ParrentCommentId)
                .OnDelete(DeleteBehavior.Restrict);



            base.OnModelCreating(builder);
        }
    }
}
