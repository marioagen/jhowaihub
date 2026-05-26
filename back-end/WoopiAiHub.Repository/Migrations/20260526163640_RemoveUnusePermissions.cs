using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusePermissions : Migration
    {
        private const string ObsoletePermissionDescriptions = @"
                    'permissions.descriptions.questions',
                    'permissions.descriptions.types',
                    'permissions.descriptions.quizzes',
                    'permissions.descriptions.prompts'
            ";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE Permissions
                SET [Active] = 0
                WHERE [Description] IN ({ObsoletePermissionDescriptions});
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE Permissions
                SET [Active] = 1
                WHERE [Description] IN ({ObsoletePermissionDescriptions});
            ");
        }
    }
}
