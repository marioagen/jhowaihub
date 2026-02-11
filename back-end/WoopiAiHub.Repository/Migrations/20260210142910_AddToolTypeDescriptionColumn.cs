using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddToolTypeDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ToolTypes",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.Prompt' WHERE Name = 'Prompt'");
            migrationBuilder.Sql(
                "UPDATE ToolTypes SET Description = 'tools.typeDisplay.Embeddings' WHERE Name = 'Embeddings'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.OCR' WHERE Name = 'OCR'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.N8N' WHERE Name = 'N8N'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ToolTypes");
        }
    }
}
