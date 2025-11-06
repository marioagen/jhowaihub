using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddStepToolDependenciesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Documents_DocumentId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_DocumentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Teams");

            migrationBuilder.CreateTable(
                name: "StepToolDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepToolId = table.Column<int>(type: "int", nullable: false),
                    DependsOnStepToolId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepToolDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepToolDependencies_StepTools_DependsOnStepToolId",
                        column: x => x.DependsOnStepToolId,
                        principalTable: "StepTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepToolDependencies_StepTools_StepToolId",
                        column: x => x.StepToolId,
                        principalTable: "StepTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StepToolDependencies_DependsOnStepToolId",
                table: "StepToolDependencies",
                column: "DependsOnStepToolId");

            migrationBuilder.CreateIndex(
                name: "IX_StepToolDependencies_StepToolId_DependsOnStepToolId",
                table: "StepToolDependencies",
                columns: new[] { "StepToolId", "DependsOnStepToolId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StepToolDependencies");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DocumentId",
                table: "Teams",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Documents_DocumentId",
                table: "Teams",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }
    }
}
