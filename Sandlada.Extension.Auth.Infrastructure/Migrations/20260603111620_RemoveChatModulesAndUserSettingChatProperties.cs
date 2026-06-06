using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChatModulesAndUserSettingChatProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ChatMessages and ChatSessions may not exist if they were never
            // created via migrations (e.g., only via EnsureCreated()). Use
            // DROP TABLE IF EXISTS to safely handle a fresh database.
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "ChatMessages";
                DROP TABLE IF EXISTS "ChatSessions";
                """);

            migrationBuilder.DropColumn(
                name: "AutoScrollToLatestMessageWhenResponded",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoScrollToLatestMessageWhileResponding",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PreferCtrlAndEnterForNewLine",
                table: "UserSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoScrollToLatestMessageWhenResponded",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoScrollToLatestMessageWhileResponding",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PreferCtrlAndEnterForNewLine",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Note: ChatSessions and ChatMessages table schemas are not reconstructed in Down().
            // A full rollback would require restoring from the snapshot prior to this migration.
        }
    }
}
