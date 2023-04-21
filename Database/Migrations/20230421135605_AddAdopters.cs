using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAdopters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                 name: "Adopters",
                 columns: table => new
                 {
                     Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                     UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Address_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Address_Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Address_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Address_Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Address_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                     Status = table.Column<int>(type: "int", nullable: false)
                 },
                 constraints: table =>
                 {
                     table.PrimaryKey("PK_Adopters", x => x.Id);
                 });

            migrationBuilder.CreateTable(
                name: "Verifications",
                columns: table => new
                {
                    AdopterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShelterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verifications", x => new { x.AdopterId, x.ShelterId });
                    table.ForeignKey(
                        name: "FK_Verifications_Adopters_AdopterId",
                        column: x => x.AdopterId,
                        principalTable: "Adopters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Verifications_Shelters_ShelterId",
                        column: x => x.ShelterId,
                        principalTable: "Shelters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_ShelterId",
                table: "Verifications",
                column: "ShelterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Verifications");

            migrationBuilder.DropTable(
                name: "Adopters");
        }
    }
}
