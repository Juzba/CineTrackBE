using CineTrackBE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
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

                 new User() { Id = "id-Juzba", UserName = "Juzba@gmail.com", NormalizedUserName = "JUZBA@GMAIL.COM", Email = "Juzba@gmail.com", NormalizedEmail = "JUZBA@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEOadgFzBJnpnkBkmi8SqFcuYgy60qk0ZBrgllZ0PPoVBypQav6KsXimrjBfiPVo6Mw==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" },
                 new User() { Id = "id-Katka", UserName = "Katka@gmail.com", NormalizedUserName = "KATKA@GMAIL.COM", Email = "Katka@gmail.com", NormalizedEmail = "KATKA@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEJaTtbyo9uZ+7zhBsqPgOSRVqq81uC1HilQAFs30aTxQs18hzOp3e9o7jZMtt3nTow==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" },
                 new User() { Id = "id-Karel", UserName = "Karel@gmail.com", NormalizedUserName = "KAREL@GMAIL.COM", Email = "Karel@gmail.com", NormalizedEmail = "KAREL@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEI3e/eOUTskYsHiohjGn7iVPezNTxmLT5XjporF7MfKyPsdcioNgrAJkTmk5H1c+IQ==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" }

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
