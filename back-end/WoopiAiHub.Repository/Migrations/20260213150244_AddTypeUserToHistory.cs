using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeUserToHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DocumentHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "DocumentHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_UserId",
                table: "DocumentHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentHistories_Users_UserId",
                table: "DocumentHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentHistories_Users_UserId",
                table: "DocumentHistories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_UserId",
                table: "DocumentHistories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DocumentHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DocumentHistories");
        }
    }
}
