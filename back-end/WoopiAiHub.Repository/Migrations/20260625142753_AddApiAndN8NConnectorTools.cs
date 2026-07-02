using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddApiAndN8NConnectorTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @ToolDataIdTexto INT;
                DECLARE @ToolTypeIdApi INT;
                DECLARE @ToolTypeIdN8N INT;

                SELECT @ToolDataIdTexto = Id FROM ToolDatas WHERE [Name] = 'Texto';
                SELECT @ToolTypeIdApi = Id FROM ToolTypes WHERE [Name] = 'API';
                SELECT @ToolTypeIdN8N = Id FROM ToolTypes WHERE [Name] = 'N8N';

                IF @ToolTypeIdApi IS NOT NULL
                   AND @ToolDataIdTexto IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM Tools WHERE [Name] = 'API')
                BEGIN
                    INSERT INTO Tools (Name, IsActive, ToolTypeId, InputDataId, OutputDataId, Created, IsEditableInput)
                    VALUES ('API', 1, @ToolTypeIdApi, @ToolDataIdTexto, @ToolDataIdTexto, GETDATE(), 1);
                END

                IF @ToolTypeIdN8N IS NOT NULL
                   AND @ToolDataIdTexto IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM Tools WHERE [Name] = 'N8N')
                BEGIN
                    INSERT INTO Tools (Name, IsActive, ToolTypeId, InputDataId, OutputDataId, Created, IsEditableInput)
                    VALUES ('N8N', 1, @ToolTypeIdN8N, @ToolDataIdTexto, @ToolDataIdTexto, GETDATE(), 0);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM Tools WHERE [Name] IN ('API', 'N8N')");
        }
    }
}
