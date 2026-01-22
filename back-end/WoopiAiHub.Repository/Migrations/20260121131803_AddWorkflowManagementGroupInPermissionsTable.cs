using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowManagementGroupInPermissionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'Gestão de Esteiras')
               BEGIN
                   INSERT INTO Permissions (Name, Created, Description, [Group])
                   VALUES ('View', GETDATE(), 'Gestão de Esteiras', 'WorkflowManagement');
               END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Permissions WHERE [Name] = 'View' AND [Group] = 'WorkflowManagement';");
        }
    }
}
