using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeferUserProfileCompletionAndAddEmailRebindVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UniqueNameNormalized",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueNameNormalized",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueName",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayNameNormalized",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "EmailRebindVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetEmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    TargetEmailAddressNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    VerificationCodeHash = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    RequestCountDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRebindVerifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users",
                column: "DisplayNameNormalized",
                unique: true,
                filter: "\"DisplayNameNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniqueNameNormalized",
                table: "Users",
                column: "UniqueNameNormalized",
                unique: true,
                filter: "\"UniqueNameNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRebindVerifications_UserId",
                table: "EmailRebindVerifications",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailRebindVerifications");

            migrationBuilder.DropIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UniqueNameNormalized",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueNameNormalized",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UniqueName",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayNameNormalized",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users",
                column: "DisplayNameNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniqueNameNormalized",
                table: "Users",
                column: "UniqueNameNormalized",
                unique: true);
        }
    }
}
