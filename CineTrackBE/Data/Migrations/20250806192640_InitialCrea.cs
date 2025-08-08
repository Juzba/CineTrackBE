using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTrackBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCrea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Juzba",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "JUZBA@GMAIL.COM", "Juzba@gmail.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Karel",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "KAREL@GMAIL.COM", "Karel@gmail.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Katka",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "KATKA@GMAIL.COM", "Katka@gmail.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Juzba",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "JUZBA", "Juzba" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Karel",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "KAREL", "Karel" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Katka",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "KATKA", "Katka" });
        }
    }
}
