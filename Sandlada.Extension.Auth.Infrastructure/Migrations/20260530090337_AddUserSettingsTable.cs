using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseTone = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Formal"),
                    SourceColorArgb = table.Column<uint>(type: "INTEGER", nullable: false, defaultValue: 4278221012u),
                    IsDarkMode = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ContrastLevel = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ThemeVariantCode = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)1),
                    PreferCtrlAndEnterForNewLine = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AutoScrollToLatestMessageWhenResponded = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AutoScrollToLatestMessageWhileResponding = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
