using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUser : Migration
    {
        private const string CardsTableName = "Cards";
        private const string AssignedUserIdColumnName = "AssignedUserId";
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: AssignedUserIdColumnName,
                table: CardsTableName,
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_AssignedUserId",
                table: CardsTableName,
                column: AssignedUserIdColumnName);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Users_AssignedUserId",
                table: CardsTableName,
                column: AssignedUserIdColumnName,
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Users_AssignedUserId",
                table: CardsTableName);

            migrationBuilder.DropIndex(
                name: "IX_Cards_AssignedUserId",
                table: CardsTableName);

            migrationBuilder.DropColumn(
                name: AssignedUserIdColumnName,
                table: CardsTableName);
        }
    }
}
