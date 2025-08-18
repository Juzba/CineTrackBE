using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTrackBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "The Lord of the Rings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "The Lord of the Rings: The Fellowship of the Ring");
        }
    }
}
