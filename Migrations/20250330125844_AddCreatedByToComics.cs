using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettruyenRemake.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToComics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "comics",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_comics_created_by",
                table: "comics",
                column: "created_by");

            migrationBuilder.AddForeignKey(
                name: "FK_comics_users_created_by",
                table: "comics",
                column: "created_by",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comics_users_created_by",
                table: "comics");

            migrationBuilder.DropIndex(
                name: "IX_comics_created_by",
                table: "comics");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "comics");
        }
    }
}
