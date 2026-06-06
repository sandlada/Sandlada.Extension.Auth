using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SlimUserProfileAddMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "UserProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "UserProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "BirthYear",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: true);
        }
    }
}