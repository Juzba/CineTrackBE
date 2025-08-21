using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTrackBE.Migrations
{
    /// <inheritdoc />
    public partial class AddTestUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "id-Test", 0, "", "Test@gmail.com", true, false, null, "TEST@GMAIL.COM", "TEST@GMAIL.COM", "AQAAAAIAAYagAAAAEBvAv3CvOSPyH4EJPSccHhl/yxgkUq0IjtRpyhwgHEnj4u855YIIotXad9BCghJ4nQ==", null, false, "", false, "Test@gmail.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "AdminRoleId-51sa9-sdd18", "id-Test" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "AdminRoleId-51sa9-sdd18", "id-Test" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "id-Test");
        }
    }
}
