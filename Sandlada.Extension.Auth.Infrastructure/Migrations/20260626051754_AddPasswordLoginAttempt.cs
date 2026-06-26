using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordLoginAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordLoginAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    EmailAddressNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    FailedAttemptCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LockoutEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RequestCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    RequestCountDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastFailedAttemptAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordLoginAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordLoginAttempts_EmailAddressNormalized",
                table: "PasswordLoginAttempts",
                column: "EmailAddressNormalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordLoginAttempts");
        }
    }
}
