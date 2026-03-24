using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'permissions.descriptions.auditor')
               BEGIN
                   INSERT INTO Permissions (Name, Created, Description, [Group])
                   VALUES ('View', GETDATE(), 'permissions.descriptions.auditor', 'Auditor');
               END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Permissions WHERE [Name] = 'View' AND [Group] = 'Auditor';");
        }
    }
}
