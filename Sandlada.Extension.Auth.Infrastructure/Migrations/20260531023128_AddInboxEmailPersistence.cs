using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandlada.Extension.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxEmailPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Mailbox = table.Column<string>(type: "TEXT", nullable: false),
                    MailboxNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    Folder = table.Column<string>(type: "TEXT", nullable: false),
                    FolderNormalized = table.Column<string>(type: "TEXT", nullable: false),
                    ImapUid = table.Column<uint>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    FromAddress = table.Column<string>(type: "TEXT", nullable: false),
                    ToAddress = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TextBody = table.Column<string>(type: "TEXT", nullable: false),
                    HtmlBody = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxEmailAttachmentMetadatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InboxEmailId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxEmailAttachmentMetadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxEmailAttachmentMetadatas_InboxEmails_InboxEmailId",
                        column: x => x.InboxEmailId,
                        principalTable: "InboxEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxEmailAttachmentMetadatas_InboxEmailId",
                table: "InboxEmailAttachmentMetadatas",
                column: "InboxEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxEmails_MailboxNormalized_FolderNormalized_ImapUid",
                table: "InboxEmails",
                columns: new[] { "MailboxNormalized", "FolderNormalized", "ImapUid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxEmails_ReceivedAt",
                table: "InboxEmails",
                column: "ReceivedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxEmailAttachmentMetadatas");

            migrationBuilder.DropTable(
                name: "InboxEmails");
        }
    }
}
