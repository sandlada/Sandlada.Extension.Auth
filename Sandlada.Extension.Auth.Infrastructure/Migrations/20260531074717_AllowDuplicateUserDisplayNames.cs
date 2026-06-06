using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateUserDisplayNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users",
                column: "DisplayNameNormalized",
                filter: "\"DisplayNameNormalized\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users",
                column: "DisplayNameNormalized",
                unique: true,
                filter: "\"DisplayNameNormalized\" IS NOT NULL");
        }
    }
}
