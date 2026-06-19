using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeConnector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'connectors.typeDisplay.Prompt'    WHERE Description = 'tools.typeDisplay.Prompt'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'connectors.typeDisplay.Embeddings' WHERE Description = 'tools.typeDisplay.Embeddings'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'connectors.typeDisplay.OCR'        WHERE Description = 'tools.typeDisplay.OCR'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'connectors.typeDisplay.N8N'        WHERE Description = 'tools.typeDisplay.N8N'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'connectors.typeDisplay.Api'        WHERE Description = 'tools.typeDisplay.Api'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'connectors.typeDisplay.Quiz'       WHERE Description = 'tools.typeDisplay.Quiz'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.Prompt'    WHERE Description = 'connectors.typeDisplay.Prompt'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.Embeddings' WHERE Description = 'connectors.typeDisplay.Embeddings'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.OCR'        WHERE Description = 'connectors.typeDisplay.OCR'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.N8N'        WHERE Description = 'connectors.typeDisplay.N8N'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.Api'        WHERE Description = 'connectors.typeDisplay.Api'");
            migrationBuilder.Sql("UPDATE ToolTypes SET Description = 'tools.typeDisplay.Quiz'       WHERE Description = 'connectors.typeDisplay.Quiz'");
        }
    }
}
