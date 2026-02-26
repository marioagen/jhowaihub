using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_DocumentBatch_Table_And_HasBatch_Flag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBatch",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DocumentBatchId",
                table: "Cards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentBatchs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentBatchs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DocumentBatchId",
                table: "Cards",
                column: "DocumentBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_DocumentBatchs_DocumentBatchId",
                table: "Cards",
                column: "DocumentBatchId",
                principalTable: "DocumentBatchs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_DocumentBatchs_DocumentBatchId",
                table: "Cards");

            migrationBuilder.DropTable(
                name: "DocumentBatchs");

            migrationBuilder.DropIndex(
                name: "IX_Cards_DocumentBatchId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "HasBatch",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DocumentBatchId",
                table: "Cards");
        }
    }
}
