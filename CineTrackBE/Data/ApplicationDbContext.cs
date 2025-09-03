using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        private readonly bool _enableSeeding;
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<FilmGenre> FilmGenres { get; set; }
        public DbSet<Genre> Genre { get; set; }
        public DbSet<Rating> Ratings { get; set; }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool enableSeeding = true)
            : base(options)
        {
            _enableSeeding = enableSeeding;
        }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (_enableSeeding)
            {
                // Seed initial data //
                SeedData.Seed(builder);
            }


            builder.Entity<Comment>()
                .HasOne(p => p.Rating)
                .WithOne(p => p.Comment)
                .HasForeignKey<Rating>(p => p.CommentId)
                .IsRequired();



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


            base.OnModelCreating(builder);
        }
    }
}
