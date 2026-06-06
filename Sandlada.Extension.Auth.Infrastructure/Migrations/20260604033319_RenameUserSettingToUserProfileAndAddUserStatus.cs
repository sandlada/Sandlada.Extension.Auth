using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserSettingToUserProfileAndAddUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayNameNormalized",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "Enabled");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceColorArgb = table.Column<uint>(type: "INTEGER", nullable: false, defaultValue: 4278221012u),
                    IsDarkMode = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ContrastLevel = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ThemeVariantCode = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)1),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    Nickname = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "unknown"),
                    BirthYear = table.Column<uint>(type: "INTEGER", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayNameNormalized",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContrastLevel = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomizationInstructions = table.Column<string>(type: "TEXT", nullable: false),
                    IsDarkMode = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PreferMoreDetailed = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Default"),
                    PreferMoreEmoji = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Default"),
                    PreferMoreHumanistic = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Default"),
                    PreferMoreListAndTable = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Default"),
                    PreferredLanguage = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseTone = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Formal"),
                    SourceColorArgb = table.Column<uint>(type: "INTEGER", nullable: false, defaultValue: 4278221012u),
                    ThemeVariantCode = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)1),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    YourBirthYear = table.Column<uint>(type: "INTEGER", nullable: true),
                    YourGender = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "unknown"),
                    YourName = table.Column<string>(type: "TEXT", nullable: true),
                    YourNickname = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users",
                column: "DisplayNameNormalized",
                filter: "\"DisplayNameNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }
    }
}
