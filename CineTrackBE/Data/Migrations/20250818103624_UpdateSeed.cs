using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CineTrackBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Films",
                columns: new[] { "Id", "Description", "Director", "ImageFileName", "Name", "ReleaseDate" },
                values: new object[,]
                {
                    { 11, "Epický příběh mafiánské rodiny Corleonů, který sleduje vzestup Michaela Corleona z neochotného outsidera na nemilosrdného mafiánského bosse.", "Francis Ford Coppola", "Godfather.jpg", "The Godfather", new DateTime(1972, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, "Paleontologové jsou pozváni do úžasného tematického parku obydleného vzkříšenými dinosaury, ale brzy zjistí, že bezpečnostní systémy selhávají.", "Steven Spielberg", "JurassicPark.jpg", "Jurassic Park", new DateTime(1993, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 13, "Mladá agentka FBI hledá pomoc uvězněného kanibala, aby chytila sériového vraha, který stahuje své oběti z kůže.", "Jonathan Demme", "SilenceOfTheLambs.jpg", "The Silence of the Lambs", new DateTime(1991, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 14, "Epický milostný příběh zasazený do pozadí tragické plavby luxusního parníku Titanic na jeho první a poslední cestě.", "James Cameron", "Titanic.jpg", "Titanic", new DateTime(1997, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 15, "Mladý hobbit a jeho přátelé se vydávají na nebezpečnou cestu, aby zničili mocný prsten a zachránili Středozem před temným pánem Sauronem.", "Peter Jackson", "LordOfTheRings.jpg", "The Lord of the Rings: The Fellowship of the Ring", new DateTime(2001, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 16, "Nespokojený úředník vytvoří podzemní bojový klub jako radikální formu psychoterapie, ale věci se vymknou kontrole.", "David Fincher", "FightClub.jpg", "Fight Club", new DateTime(1999, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 17, "Tým superhrdinů se musí spojit, aby zachránili Zemi před invazí mimozemšťanů vedenou Lokiho armádou.", "Joss Whedon", "Avengers.jpg", "The Avengers", new DateTime(2012, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 18, "Bývalý římský generál se snaží pomstít vraždu své rodiny tím, že se stane gladiátorem a vyzve zkorumpovaného císaře.", "Ridley Scott", "Gladiator.jpg", "Gladiator", new DateTime(2000, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 19, "Život dozorců na cele smrti je ovlivněn příchodem záhadného vězně s nadpřirozenými schopnostmi léčit.", "Frank Darabont", "GreenMile.jpg", "The Green Mile", new DateTime(1999, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 20, "Tým průzkumníků cestuje červí dírou ve vesmíru v pokusu zajistit přežití lidstva.", "Christopher Nolan", "Interstellar.jpg", "Interstellar", new DateTime(2014, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "FilmGenres",
                columns: new[] { "FilmId", "GenreId" },
                values: new object[,]
                {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 11, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 11, 5 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 12, 4 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 12, 6 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 13, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 13, 5 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 14, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 14, 3 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 15, 4 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 15, 6 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 16, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 16, 5 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 17, 4 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 17, 6 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 18, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 18, 4 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 19, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 19, 6 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 20, 1 });

            migrationBuilder.DeleteData(
                table: "FilmGenres",
                keyColumns: new[] { "FilmId", "GenreId" },
                keyValues: new object[] { 20, 6 });

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}
