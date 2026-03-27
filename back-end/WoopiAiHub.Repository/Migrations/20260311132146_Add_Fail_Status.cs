using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_Fail_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Name] = 'Fail')
                BEGIN
                    INSERT INTO [Status] ([Name], Created, Color, Label) VALUES 
                    ('Fail', GETDATE(), '#D10000', 'workflow.statusList.fail')
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [Status] WHERE [Name] = 'Fail'");
        }
    }
}
