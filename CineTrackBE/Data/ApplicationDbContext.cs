using CineTrackBE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Data
{
    public class ApplicationDbContext : IdentityDbContext
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

            builder.Entity<IdentityRole>().HasData
            (
                new IdentityRole() { Id = "AdminRoleId-51sa9-sdd18", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole() { Id = "UserRoleId-54sa9-sda87", Name = "User", NormalizedName = "USER" }
            );

            builder.Entity<User>().HasData
            (

                 new User() { Id = "id-Juzba", UserName = "Juzba", NormalizedUserName = "JUZBA", Email = "Juzba@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAEEtE8TBZNfq5WYGGHoaa8n5kX2UlsEJebHPUJRWxDjrEx5VW3NXeNQbL8tN5oIYJng==", EmailConfirmed = true, SecurityStamp = "security-stamp1-#dsad", ConcurrencyStamp = "concurrency-stamp1-#sfdf" },
                 new User() { Id = "id-Katka", UserName = "Katka", NormalizedUserName = "KATKA", Email = "Katka@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAECOcv9Vu94/XH41PY720P1vJF0oNN2xqPR45p0z0aZBj+ei8Ouxg8xlYBwxxhcs0XQ==", EmailConfirmed = true, SecurityStamp = "security-stamp2-#oioi", ConcurrencyStamp = "concurrency-stamp2-#qwwe" },
                 new User() { Id = "id-Karel", UserName = "Karel", NormalizedUserName = "KAREL", Email = "Karel@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAEGTULgpM7dQXOTBJWKDizbyPzwYtj8ixfvSfbCVp7kJ5V+V39280pzDa42v/cZ1SiA==", EmailConfirmed = true, SecurityStamp = "security-stamp3-#dser", ConcurrencyStamp = "concurrency-stamp3-#wwsh" }

            );

            builder.Entity<IdentityUserRole<string>>().HasData
            (
                new IdentityUserRole<string>() { RoleId = "AdminRoleId-51sa9-sdd18", UserId = "id-Juzba" },
                new IdentityUserRole<string>() { RoleId = "AdminRoleId-51sa9-sdd18", UserId = "id-Katka" },
                new IdentityUserRole<string>() { RoleId = "UserRoleId-54sa9-sda87", UserId = "id-Karel" }
            );



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
