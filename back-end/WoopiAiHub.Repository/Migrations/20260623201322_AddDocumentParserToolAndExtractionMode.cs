using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentParserToolAndExtractionMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantLlmModelSettings",
                columns: table => new
                {
                    Scope = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(150)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedByEmail = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLlmModelSettings", x => x.Scope);
                });

            migrationBuilder.AddColumn<string>(
                name: "Extraction_Mode",
                table: "Documents",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ToolTypes WHERE [Name] = 'Parser')
                BEGIN
                    INSERT INTO ToolTypes (Name, Description, IsActive, Created)
                    VALUES ('Parser', 'connectors.typeDisplay.Parser', 1, GETDATE());
                END
            ");

            migrationBuilder.Sql(@"
                DECLARE @ToolDataIdPdf INT;
                DECLARE @ToolDataIdTexto INT;
                DECLARE @ToolTypeIdParser INT;

                SELECT @ToolDataIdPdf = Id FROM ToolDatas WHERE [Name] = 'PDF';
                SELECT @ToolDataIdTexto = Id FROM ToolDatas WHERE [Name] = 'Texto';
                SELECT @ToolTypeIdParser = Id FROM ToolTypes WHERE [Name] = 'Parser';

                IF @ToolTypeIdParser IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM Tools WHERE [Name] = 'Document Parser')
                BEGIN
                    INSERT INTO Tools (Name, IsActive, ToolTypeId, InputDataId, OutputDataId, Created)
                    VALUES ('Document Parser', 1, @ToolTypeIdParser, @ToolDataIdPdf, @ToolDataIdTexto, GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM Tools WHERE [Name] = 'Document Parser'");
            migrationBuilder.Sql(@"DELETE FROM ToolTypes WHERE [Name] = 'Parser'");

            migrationBuilder.DropColumn(
                name: "Extraction_Mode",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "TenantLlmModelSettings");
        }
    }
}
