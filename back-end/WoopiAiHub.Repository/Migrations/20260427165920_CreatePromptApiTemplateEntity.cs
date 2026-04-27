using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CreatePromptApiTemplateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableAccessToMcp",
                table: "Prompts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ApiTemplates",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAccessFromMcp",
                table: "ApiTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PromptApiTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromptId = table.Column<int>(type: "int", nullable: false),
                    ApiTemplateId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptApiTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptApiTemplates_ApiTemplates_ApiTemplateId",
                        column: x => x.ApiTemplateId,
                        principalTable: "ApiTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromptApiTemplates_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptApiTemplates_ApiTemplateId",
                table: "PromptApiTemplates",
                column: "ApiTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptApiTemplates_PromptId",
                table: "PromptApiTemplates",
                column: "PromptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptApiTemplates");

            migrationBuilder.DropColumn(
                name: "EnableAccessToMcp",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ApiTemplates");

            migrationBuilder.DropColumn(
                name: "EnableAccessFromMcp",
                table: "ApiTemplates");
        }
    }
}
