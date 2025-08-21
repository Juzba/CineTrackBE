using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Data
{
    public static class SeedData
    {


        public static void Seed(ModelBuilder builder)
        {



            builder.Entity<IdentityRole>().HasData
            (
                new IdentityRole() { Id = "AdminRoleId-51sa9-sdd18", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole() { Id = "UserRoleId-54sa9-sda87", Name = "User", NormalizedName = "USER" }
            );

            builder.Entity<ApplicationUser>().HasData
            (

                 new ApplicationUser() { Id = "id-Juzba", UserName = "Juzba@gmail.com", NormalizedUserName = "JUZBA@GMAIL.COM", Email = "Juzba@gmail.com", NormalizedEmail = "JUZBA@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEOadgFzBJnpnkBkmi8SqFcuYgy60qk0ZBrgllZ0PPoVBypQav6KsXimrjBfiPVo6Mw==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" },
                 new ApplicationUser() { Id = "id-Katka", UserName = "Katka@gmail.com", NormalizedUserName = "KATKA@GMAIL.COM", Email = "Katka@gmail.com", NormalizedEmail = "KATKA@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEJaTtbyo9uZ+7zhBsqPgOSRVqq81uC1HilQAFs30aTxQs18hzOp3e9o7jZMtt3nTow==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" },
                 new ApplicationUser() { Id = "id-Karel", UserName = "Karel@gmail.com", NormalizedUserName = "KAREL@GMAIL.COM", Email = "Karel@gmail.com", NormalizedEmail = "KAREL@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEI3e/eOUTskYsHiohjGn7iVPezNTxmLT5XjporF7MfKyPsdcioNgrAJkTmk5H1c+IQ==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" },
                 new ApplicationUser() { Id = "id-Test", UserName = "Test@gmail.com", NormalizedUserName = "TEST@GMAIL.COM", Email = "Test@gmail.com", NormalizedEmail = "TEST@GMAIL.COM", PasswordHash = "AQAAAAIAAYagAAAAEBvAv3CvOSPyH4EJPSccHhl/yxgkUq0IjtRpyhwgHEnj4u855YIIotXad9BCghJ4nQ==", EmailConfirmed = true, ConcurrencyStamp = "", SecurityStamp = "" }

            );

            builder.Entity<IdentityUserRole<string>>().HasData
            (
                new IdentityUserRole<string>() { RoleId = "AdminRoleId-51sa9-sdd18", UserId = "id-Juzba" },
                new IdentityUserRole<string>() { RoleId = "AdminRoleId-51sa9-sdd18", UserId = "id-Katka" },
                new IdentityUserRole<string>() { RoleId = "UserRoleId-54sa9-sda87", UserId = "id-Karel" },
                new IdentityUserRole<string>() { RoleId = "AdminRoleId-51sa9-sdd18", UserId = "id-Test" }
            );


            builder.Entity<Genre>().HasData
            (
                new Genre() { Id = 1, Name = "Drama" },
                new Genre() { Id = 2, Name = "Horror" },
                new Genre() { Id = 3, Name = "Comedy" },
                new Genre() { Id = 4, Name = "Action" },
                new Genre() { Id = 5, Name = "Thriller" },
                new Genre() { Id = 6, Name = "Sci-fi" }
            );


            builder.Entity<Film>().HasData
            (
                  new Film()
                  {
                      Id = 1,
                      Name = "Hvězdná pěchota",
                      Description = "Sledujeme mladé rekruty v boji proti mimozemským pavoukům, zatímco režisér Paul Verhoeven chytře kritizuje militarismus a propagandu.",
                      Director = "Paul Verhoeven",
                      ReleaseDate = new DateTime(1997, 07, 09),
                      ImageFileName = "StarshipTroopers.jpg"
                  },
                  new Film()
                  {
                      Id = 2,
                      Name = "John Wick",
                      Description = "Bývalý nájemný vrah John Wick rozpoutá krvavou cestu pomsty poté, co mu ruští gangsteři ukradnou auto a zabijí jeho milovaného psa, poslední dar od jeho zesnulé ženy.",
                      Director = "Chad Stahelski",
                      ReleaseDate = new DateTime(2014, 12, 21),
                      ImageFileName = "JohnWick.jpg"
                  },
                  new Film()
                  {
                      Id = 3,
                      Name = "Inception",
                      Description = "Dom Cobb je zručný zloděj, který krade tajemství z podvědomí během snění. Dostává nabídku na poslední job, který by mu mohl vrátit jeho starý život.",
                      Director = "Christopher Nolan",
                      ReleaseDate = new DateTime(2010, 07, 16),
                      ImageFileName = "Inception.jpg"
                  },
                  new Film()
                  {
                      Id = 4,
                      Name = "Pulp Fiction",
                      Description = "Kultovní film Quentina Tarantina propojuje několik příběhů zločinců v Los Angeles.",
                      Director = "Quentin Tarantino",
                      ReleaseDate = new DateTime(1994, 10, 14),
                      ImageFileName = "PulpFiction.jpg"
                  },
                  new Film()
                  {
                      Id = 5,
                      Name = "The Shawshank Redemption",
                      Description = "Dva uvěznění muži během několika let najdou útěchu a případné vykoupení skrze činy obyčejné slušnosti.",
                      Director = "Frank Darabont",
                      ReleaseDate = new DateTime(1994, 09, 23),
                      ImageFileName = "ShawshankRedemption.jpg"
                  },
                  new Film()
                  {
                      Id = 6,
                      Name = "The Dark Knight",
                      Description = "Batman, Gordon a Harvey Dent jsou nuceni čelit chaosu rozpoutanému v Gothamu anarchistickým kriminálním géniem známým jako Joker.",
                      Director = "Christopher Nolan",
                      ReleaseDate = new DateTime(2008, 07, 18),
                      ImageFileName = "DarkKnight.jpg"
                  },
                  new Film()
                  {
                      Id = 7,
                      Name = "Forrest Gump",
                      Description = "Příběh Forresta Gumpa, muže s nízkým IQ, který se nevědomky účastní mnoha historických událostí ve 20. století.",
                      Director = "Robert Zemeckis",
                      ReleaseDate = new DateTime(1994, 07, 06),
                      ImageFileName = "ForrestGump.jpg"
                  },
                  new Film()
                  {
                      Id = 8,
                      Name = "The Matrix",
                      Description = "Programátor počítačů objeví šokující pravdu o realitě a svém místě v ní.",
                      Director = "The Wachowskis",
                      ReleaseDate = new DateTime(1999, 03, 31),
                      ImageFileName = "Matrix.jpg"
                  },
                  new Film()
                  {
                      Id = 9,
                      Name = "Schindler's List",
                      Description = "V německy okupovaném Polsku během 2. světové války se Oskar Schindler postupně stává svědomitým a zachraňuje životy více než tisíce židovských uprchlíků.",
                      Director = "Steven Spielberg",
                      ReleaseDate = new DateTime(1993, 12, 15),
                      ImageFileName = "SchindlersList.jpg"
                  },
                  new Film()
                  {
                      Id = 10,
                      Name = "Goodfellas",
                      Description = "Příběh Henryho Hilla a jeho života v mafii, který pokrývá jeho vztah s jeho ženou Karen Hill a jeho partnery v zločinu Jimmy Conwayem a Tommy DeVitem.",
                      Director = "Martin Scorsese",
                      ReleaseDate = new DateTime(1990, 09, 19),
                      ImageFileName = "Goodfellas.jpg"
                  },
                   new Film()
                   {
                       Id = 11,
                       Name = "The Godfather",
                       Description = "Epický příběh mafiánské rodiny Corleonů, který sleduje vzestup Michaela Corleona z neochotného outsidera na nemilosrdného mafiánského bosse.",
                       Director = "Francis Ford Coppola",
                       ReleaseDate = new DateTime(1972, 03, 24),
                       ImageFileName = "Godfather.jpg"
                   },
            new Film()
            {
                Id = 12,
                Name = "Jurassic Park",
                Description = "Paleontologové jsou pozváni do úžasného tematického parku obydleného vzkříšenými dinosaury, ale brzy zjistí, že bezpečnostní systémy selhávají.",
                Director = "Steven Spielberg",
                ReleaseDate = new DateTime(1993, 06, 11),
                ImageFileName = "JurassicPark.jpg"
            },
            new Film()
            {
                Id = 13,
                Name = "The Silence of the Lambs",
                Description = "Mladá agentka FBI hledá pomoc uvězněného kanibala, aby chytila sériového vraha, který stahuje své oběti z kůže.",
                Director = "Jonathan Demme",
                ReleaseDate = new DateTime(1991, 02, 14),
                ImageFileName = "SilenceOfTheLambs.jpg"
            },
            new Film()
            {
                Id = 14,
                Name = "Titanic",
                Description = "Epický milostný příběh zasazený do pozadí tragické plavby luxusního parníku Titanic na jeho první a poslední cestě.",
                Director = "James Cameron",
                ReleaseDate = new DateTime(1997, 12, 19),
                ImageFileName = "Titanic.jpg"
            },
            new Film()
            {
                Id = 15,
                Name = "The Lord of the Rings",
                Description = "Mladý hobbit a jeho přátelé se vydávají na nebezpečnou cestu, aby zničili mocný prsten a zachránili Středozem před temným pánem Sauronem.",
                Director = "Peter Jackson",
                ReleaseDate = new DateTime(2001, 12, 19),
                ImageFileName = "LordOfTheRings.jpg"
            },
            new Film()
            {
                Id = 16,
                Name = "Fight Club",
                Description = "Nespokojený úředník vytvoří podzemní bojový klub jako radikální formu psychoterapie, ale věci se vymknou kontrole.",
                Director = "David Fincher",
                ReleaseDate = new DateTime(1999, 10, 15),
                ImageFileName = "FightClub.jpg"
            },
            new Film()
            {
                Id = 17,
                Name = "The Avengers",
                Description = "Tým superhrdinů se musí spojit, aby zachránili Zemi před invazí mimozemšťanů vedenou Lokiho armádou.",
                Director = "Joss Whedon",
                ReleaseDate = new DateTime(2012, 05, 04),
                ImageFileName = "Avengers.jpg"
            },
            new Film()
            {
                Id = 18,
                Name = "Gladiator",
                Description = "Bývalý římský generál se snaží pomstít vraždu své rodiny tím, že se stane gladiátorem a vyzve zkorumpovaného císaře.",
                Director = "Ridley Scott",
                ReleaseDate = new DateTime(2000, 05, 05),
                ImageFileName = "Gladiator.jpg"
            },
            new Film()
            {
                Id = 19,
                Name = "The Green Mile",
                Description = "Život dozorců na cele smrti je ovlivněn příchodem záhadného vězně s nadpřirozenými schopnostmi léčit.",
                Director = "Frank Darabont",
                ReleaseDate = new DateTime(1999, 12, 10),
                ImageFileName = "GreenMile.jpg"
            },
            new Film()
            {
                Id = 20,
                Name = "Interstellar",
                Description = "Tým průzkumníků cestuje červí dírou ve vesmíru v pokusu zajistit přežití lidstva.",
                Director = "Christopher Nolan",
                ReleaseDate = new DateTime(2014, 11, 07),
                ImageFileName = "Interstellar.jpg"
            }
            );


            builder.Entity<FilmGenre>().HasData
                (
                new FilmGenre() { FilmId = 1, GenreId = 4 },
                new FilmGenre() { FilmId = 1, GenreId = 6 },
                new FilmGenre() { FilmId = 2, GenreId = 4 },
                new FilmGenre() { FilmId = 2, GenreId = 5 },
                new FilmGenre() { FilmId = 3, GenreId = 4 },
                new FilmGenre() { FilmId = 3, GenreId = 6 },
                new FilmGenre() { FilmId = 4, GenreId = 1 },
                new FilmGenre() { FilmId = 4, GenreId = 3 },
                new FilmGenre() { FilmId = 5, GenreId = 1 },
                new FilmGenre() { FilmId = 6, GenreId = 4 },
                new FilmGenre() { FilmId = 6, GenreId = 1 },
                new FilmGenre() { FilmId = 7, GenreId = 1 },
                new FilmGenre() { FilmId = 7, GenreId = 3 },
                new FilmGenre() { FilmId = 8, GenreId = 4 },
                new FilmGenre() { FilmId = 8, GenreId = 6 },
                new FilmGenre() { FilmId = 9, GenreId = 1 },
                new FilmGenre() { FilmId = 10, GenreId = 1 },
                new FilmGenre() { FilmId = 10, GenreId = 5 },
                new FilmGenre() { FilmId = 11, GenreId = 1 },
                new FilmGenre() { FilmId = 11, GenreId = 5 },
                new FilmGenre() { FilmId = 12, GenreId = 4 },
                new FilmGenre() { FilmId = 12, GenreId = 6 },
                new FilmGenre() { FilmId = 13, GenreId = 1 },
                new FilmGenre() { FilmId = 13, GenreId = 5 },
                new FilmGenre() { FilmId = 14, GenreId = 1 },
                new FilmGenre() { FilmId = 14, GenreId = 3 },
                new FilmGenre() { FilmId = 15, GenreId = 4 },
                new FilmGenre() { FilmId = 15, GenreId = 6 },
                new FilmGenre() { FilmId = 16, GenreId = 1 },
                new FilmGenre() { FilmId = 16, GenreId = 5 },
                new FilmGenre() { FilmId = 17, GenreId = 4 },
                new FilmGenre() { FilmId = 17, GenreId = 6 },
                new FilmGenre() { FilmId = 18, GenreId = 1 },
                new FilmGenre() { FilmId = 18, GenreId = 4 },
                new FilmGenre() { FilmId = 19, GenreId = 1 },
                new FilmGenre() { FilmId = 19, GenreId = 6 },
                new FilmGenre() { FilmId = 20, GenreId = 1 },
                new FilmGenre() { FilmId = 20, GenreId = 6 }
                );



        }


    }
}
