using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignUserSettingsPersistenceAndUpsertSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "PreferCtrlAndEnterForNewLine",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomizationInstructions",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "PreferMoreDetailed",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<string>(
                name: "PreferMoreEmoji",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<string>(
                name: "PreferMoreHumanistic",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<string>(
                name: "PreferMoreListAndTable",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<uint>(
                name: "YourBirthYear",
                table: "UserSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YourGender",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "unknown");

            migrationBuilder.AddColumn<string>(
                name: "YourName",
                table: "UserSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YourNickname",
                table: "UserSettings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomizationInstructions",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PreferMoreDetailed",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PreferMoreEmoji",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PreferMoreHumanistic",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PreferMoreListAndTable",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "YourBirthYear",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "YourGender",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "YourName",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "YourNickname",
                table: "UserSettings");

            migrationBuilder.AlterColumn<bool>(
                name: "PreferCtrlAndEnterForNewLine",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);
        }
    }
}
