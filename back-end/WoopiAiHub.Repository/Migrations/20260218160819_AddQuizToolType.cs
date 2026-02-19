using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizToolType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ToolTypes WHERE [Name] = 'Quiz')
                BEGIN
                    INSERT INTO ToolType (Name, Description, IsActive, Created)
                    VALUES ('Quiz', 'tools.typeDisplay.Quiz', 1, GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ToolType WHERE [Name] = 'Quiz'");
        }
    }
}
