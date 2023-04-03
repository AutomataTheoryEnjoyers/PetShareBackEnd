using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class Migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shelters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullShelterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAuthorized = table.Column<bool>(type: "bit", nullable: true),
                    Address_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShelterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Species = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Breed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pets_Shelters_ShelterId",
                        column: x => x.ShelterId,
                        principalTable: "Shelters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PetEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_Pets_PetEntityId",
                        column: x => x.PetEntityId,
                        principalTable: "Pets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Announcements_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Announcements_Shelters_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Shelters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AuthorId",
                table: "Announcements",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_PetEntityId",
                table: "Announcements",
                column: "PetEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_PetId",
                table: "Announcements",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_ShelterId",
                table: "Pets",
                column: "ShelterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Shelters");
        }
    }
}
