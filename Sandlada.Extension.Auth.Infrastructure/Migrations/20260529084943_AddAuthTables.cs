using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthSessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthSessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    EmailAddressNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    VerificationCodeHash = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    EmailAddressNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    UniqueName = table.Column<string>(type: "TEXT", nullable: false),
                    UniqueNameNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayNameNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthSessions_UserId",
                table: "AuthSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationVerifications_EmailAddressNormalized",
                table: "RegistrationVerifications",
                column: "EmailAddressNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayNameNormalized",
                table: "Users",
                column: "DisplayNameNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddressNormalized",
                table: "Users",
                column: "EmailAddressNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniqueNameNormalized",
                table: "Users",
                column: "UniqueNameNormalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthSessions");

            migrationBuilder.DropTable(
                name: "RegistrationVerifications");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
