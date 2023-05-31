using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class LikedAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    AdopterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.AdopterId, x.AnnouncementId });
                    table.ForeignKey(
                        name: "FK_Likes_Adopters_AdopterId",
                        column: x => x.AdopterId,
                        principalTable: "Adopters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_AnnouncementId",
                table: "Likes",
                column: "AnnouncementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");
        }
    }
}
