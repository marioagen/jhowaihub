using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_AssignedUserId",
                table: "Cards",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Users_AssignedUserId",
                table: "Cards",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Users_AssignedUserId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_AssignedUserId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Cards");
        }
    }
}
