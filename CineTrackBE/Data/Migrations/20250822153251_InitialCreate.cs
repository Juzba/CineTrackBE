using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CineTrackBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FavoriteMovies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Films",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Director = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Films", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AutorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FilmId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_AutorId",
                        column: x => x.AutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRating = table.Column<int>(type: "int", nullable: false),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ratings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilmGenres",
                columns: table => new
                {
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmGenres", x => new { x.FilmId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_FilmGenres_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmGenres_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "AdminRoleId-51sa9-sdd18", null, "Admin", "ADMIN" },
                    { "UserRoleId-54sa9-sda87", null, "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FavoriteMovies", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "id-Juzba", 0, "", "Juzba@gmail.com", true, "[]", false, null, "JUZBA@GMAIL.COM", "JUZBA@GMAIL.COM", "AQAAAAIAAYagAAAAEOadgFzBJnpnkBkmi8SqFcuYgy60qk0ZBrgllZ0PPoVBypQav6KsXimrjBfiPVo6Mw==", null, false, "", false, "Juzba@gmail.com" },
                    { "id-Karel", 0, "", "Karel@gmail.com", true, "[]", false, null, "KAREL@GMAIL.COM", "KAREL@GMAIL.COM", "AQAAAAIAAYagAAAAEI3e/eOUTskYsHiohjGn7iVPezNTxmLT5XjporF7MfKyPsdcioNgrAJkTmk5H1c+IQ==", null, false, "", false, "Karel@gmail.com" },
                    { "id-Katka", 0, "", "Katka@gmail.com", true, "[]", false, null, "KATKA@GMAIL.COM", "KATKA@GMAIL.COM", "AQAAAAIAAYagAAAAEJaTtbyo9uZ+7zhBsqPgOSRVqq81uC1HilQAFs30aTxQs18hzOp3e9o7jZMtt3nTow==", null, false, "", false, "Katka@gmail.com" },
                    { "id-Test", 0, "", "Test@gmail.com", true, "[]", false, null, "TEST@GMAIL.COM", "TEST@GMAIL.COM", "AQAAAAIAAYagAAAAEBvAv3CvOSPyH4EJPSccHhl/yxgkUq0IjtRpyhwgHEnj4u855YIIotXad9BCghJ4nQ==", null, false, "", false, "Test@gmail.com" }
                });

            migrationBuilder.InsertData(
                table: "Films",
                columns: new[] { "Id", "Description", "Director", "ImageFileName", "Name", "ReleaseDate" },
                values: new object[,]
                {
                    { 1, "Sledujeme mladé rekruty v boji proti mimozemským pavoukům, zatímco režisér Paul Verhoeven chytře kritizuje militarismus a propagandu.", "Paul Verhoeven", "StarshipTroopers.jpg", "Hvězdná pěchota", new DateTime(1997, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "Bývalý nájemný vrah John Wick rozpoutá krvavou cestu pomsty poté, co mu ruští gangsteři ukradnou auto a zabijí jeho milovaného psa, poslední dar od jeho zesnulé ženy.", "Chad Stahelski", "JohnWick.jpg", "John Wick", new DateTime(2014, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "Dom Cobb je zručný zloděj, který krade tajemství z podvědomí během snění. Dostává nabídku na poslední job, který by mu mohl vrátit jeho starý život.", "Christopher Nolan", "Inception.jpg", "Inception", new DateTime(2010, 7, 16, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "Kultovní film Quentina Tarantina propojuje několik příběhů zločinců v Los Angeles.", "Quentin Tarantino", "PulpFiction.jpg", "Pulp Fiction", new DateTime(1994, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "Dva uvěznění muži během několika let najdou útěchu a případné vykoupení skrze činy obyčejné slušnosti.", "Frank Darabont", "ShawshankRedemption.jpg", "The Shawshank Redemption", new DateTime(1994, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, "Batman, Gordon a Harvey Dent jsou nuceni čelit chaosu rozpoutanému v Gothamu anarchistickým kriminálním géniem známým jako Joker.", "Christopher Nolan", "DarkKnight.jpg", "The Dark Knight", new DateTime(2008, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, "Příběh Forresta Gumpa, muže s nízkým IQ, který se nevědomky účastní mnoha historických událostí ve 20. století.", "Robert Zemeckis", "ForrestGump.jpg", "Forrest Gump", new DateTime(1994, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, "Programátor počítačů objeví šokující pravdu o realitě a svém místě v ní.", "The Wachowskis", "Matrix.jpg", "The Matrix", new DateTime(1999, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, "V německy okupovaném Polsku během 2. světové války se Oskar Schindler postupně stává svědomitým a zachraňuje životy více než tisíce židovských uprchlíků.", "Steven Spielberg", "SchindlersList.jpg", "Schindler's List", new DateTime(1993, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, "Příběh Henryho Hilla a jeho života v mafii, který pokrývá jeho vztah s jeho ženou Karen Hill a jeho partnery v zločinu Jimmy Conwayem a Tommy DeVitem.", "Martin Scorsese", "Goodfellas.jpg", "Goodfellas", new DateTime(1990, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, "Epický příběh mafiánské rodiny Corleonů, který sleduje vzestup Michaela Corleona z neochotného outsidera na nemilosrdného mafiánského bosse.", "Francis Ford Coppola", "Godfather.jpg", "The Godfather", new DateTime(1972, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, "Paleontologové jsou pozváni do úžasného tematického parku obydleného vzkříšenými dinosaury, ale brzy zjistí, že bezpečnostní systémy selhávají.", "Steven Spielberg", "JurassicPark.jpg", "Jurassic Park", new DateTime(1993, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 13, "Mladá agentka FBI hledá pomoc uvězněného kanibala, aby chytila sériového vraha, který stahuje své oběti z kůže.", "Jonathan Demme", "SilenceOfTheLambs.jpg", "The Silence of the Lambs", new DateTime(1991, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 14, "Epický milostný příběh zasazený do pozadí tragické plavby luxusního parníku Titanic na jeho první a poslední cestě.", "James Cameron", "Titanic.jpg", "Titanic", new DateTime(1997, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 15, "Mladý hobbit a jeho přátelé se vydávají na nebezpečnou cestu, aby zničili mocný prsten a zachránili Středozem před temným pánem Sauronem.", "Peter Jackson", "LordOfTheRings.jpg", "The Lord of the Rings", new DateTime(2001, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 16, "Nespokojený úředník vytvoří podzemní bojový klub jako radikální formu psychoterapie, ale věci se vymknou kontrole.", "David Fincher", "FightClub.jpg", "Fight Club", new DateTime(1999, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 17, "Tým superhrdinů se musí spojit, aby zachránili Zemi před invazí mimozemšťanů vedenou Lokiho armádou.", "Joss Whedon", "Avengers.jpg", "The Avengers", new DateTime(2012, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 18, "Bývalý římský generál se snaží pomstít vraždu své rodiny tím, že se stane gladiátorem a vyzve zkorumpovaného císaře.", "Ridley Scott", "Gladiator.jpg", "Gladiator", new DateTime(2000, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 19, "Život dozorců na cele smrti je ovlivněn příchodem záhadného vězně s nadpřirozenými schopnostmi léčit.", "Frank Darabont", "GreenMile.jpg", "The Green Mile", new DateTime(1999, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 20, "Tým průzkumníků cestuje červí dírou ve vesmíru v pokusu zajistit přežití lidstva.", "Christopher Nolan", "Interstellar.jpg", "Interstellar", new DateTime(2014, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Genre",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Drama" },
                    { 2, "Horror" },
                    { 3, "Comedy" },
                    { 4, "Action" },
                    { 5, "Thriller" },
                    { 6, "Sci-fi" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "AdminRoleId-51sa9-sdd18", "id-Juzba" },
                    { "UserRoleId-54sa9-sda87", "id-Karel" },
                    { "AdminRoleId-51sa9-sdd18", "id-Katka" },
                    { "AdminRoleId-51sa9-sdd18", "id-Test" }
                });

            migrationBuilder.InsertData(
                table: "FilmGenres",
                columns: new[] { "FilmId", "GenreId" },
                values: new object[,]
                {
                    { 1, 4 },
                    { 1, 6 },
                    { 2, 4 },
                    { 2, 5 },
                    { 3, 4 },
                    { 3, 6 },
                    { 4, 1 },
                    { 4, 3 },
                    { 5, 1 },
                    { 6, 1 },
                    { 6, 4 },
                    { 7, 1 },
                    { 7, 3 },
                    { 8, 4 },
                    { 8, 6 },
                    { 9, 1 },
                    { 10, 1 },
                    { 10, 5 },
                    { 11, 1 },
                    { 11, 5 },
                    { 12, 4 },
                    { 12, 6 },
                    { 13, 1 },
                    { 13, 5 },
                    { 14, 1 },
                    { 14, 3 },
                    { 15, 4 },
                    { 15, 6 },
                    { 16, 1 },
                    { 16, 5 },
                    { 17, 4 },
                    { 17, 6 },
                    { 18, 1 },
                    { 18, 4 },
                    { 19, 1 },
                    { 19, 6 },
                    { 20, 1 },
                    { 20, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AutorId",
                table: "Comments",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_FilmId",
                table: "Comments",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmGenres_GenreId",
                table: "FilmGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_FilmId",
                table: "Ratings",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId",
                table: "Ratings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "FilmGenres");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Films");
        }
    }
}
