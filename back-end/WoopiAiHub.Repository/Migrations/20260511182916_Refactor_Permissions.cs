using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Permissions SET [Active] = 0
                WHERE [Description] IN (
                    'permissions.descriptions.quizzes',
                    'permissions.descriptions.management',
                    'permissions.descriptions.users',
                    'permissions.descriptions.teams',
                    'permissions.descriptions.profiles',
                    'permissions.descriptions.tools'
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO Permissions ([Name], Created, [Description], [Group], [Active]) VALUES
                ('View prompts', GETDATE(), 'permissions.descriptions.tools.prompts', 'Tools', 1),
                ('View quizzes', GETDATE(), 'permissions.descriptions.tools.quizzes', 'Tools', 1),
                ('View APIs', GETDATE(), 'permissions.descriptions.tools.apis', 'Tools', 1),
                ('View connectors', GETDATE(), 'permissions.descriptions.tools.connectors', 'Tools', 1),
                ('View users', GETDATE(), 'permissions.descriptions.management.users', 'Management', 1),
                ('View teams', GETDATE(), 'permissions.descriptions.management.teams', 'Management', 1),
                ('View profiles', GETDATE(), 'permissions.descriptions.management.profiles', 'Management', 1)
                ;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ProfilePermissions (ProfileId, PermissionId)
                SELECT 
                    Profiles.ProfileId, 
                    NewPerms.Id
                FROM (
                    SELECT DISTINCT PP.ProfileId
                    FROM ProfilePermissions PP 
                    INNER JOIN Permissions PER ON PER.Id = PP.PermissionId
                    WHERE PER.[Description] = 'permissions.descriptions.management'
                ) AS Profiles
                CROSS JOIN (
                    SELECT P.Id
                    FROM Permissions P 
                    WHERE P.[Description] IN (
                        'permissions.descriptions.management.users',
                        'permissions.descriptions.management.teams',
                        'permissions.descriptions.management.profiles'
                    )
                ) AS NewPerms
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM ProfilePermissions Existing 
                    WHERE Existing.ProfileId = Profiles.ProfileId 
                    AND Existing.PermissionId = NewPerms.Id
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ProfilePermissions (ProfileId, PermissionId)
                SELECT 
                    Profiles.ProfileId, 
                    NewPerms.Id
                FROM (
                    SELECT DISTINCT PP.ProfileId
                    FROM ProfilePermissions PP 
                    INNER JOIN Permissions PER ON PER.Id = PP.PermissionId
                    WHERE PER.[Description] = 'permissions.descriptions.tools'
                ) AS Profiles
                CROSS JOIN (
                    SELECT P.Id
                    FROM Permissions P 
                    WHERE P.[Description] IN (
                        'permissions.descriptions.tools.prompts',
                        'permissions.descriptions.tools.quizzes',
                        'permissions.descriptions.tools.apis',
                        'permissions.descriptions.tools.connectors'
                    )
                ) AS NewPerms
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM ProfilePermissions Existing 
                    WHERE Existing.ProfileId = Profiles.ProfileId 
                    AND Existing.PermissionId = NewPerms.Id
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ProfilePermissions
                WHERE PermissionId IN (
                    SELECT Id FROM Permissions
                    WHERE [Description] IN (
                        'permissions.descriptions.tools.prompts',
                        'permissions.descriptions.tools.quizzes',
                        'permissions.descriptions.tools.apis',
                        'permissions.descriptions.tools.connectors',
                        'permissions.descriptions.management.users',
                        'permissions.descriptions.management.teams',
                        'permissions.descriptions.management.profiles'
                    )
                );
            ");

            migrationBuilder.Sql(@"
                DELETE FROM Permissions
                WHERE [Description] IN (
                    'permissions.descriptions.tools.prompts',
                    'permissions.descriptions.tools.quizzes',
                    'permissions.descriptions.tools.apis',
                    'permissions.descriptions.tools.connectors',
                    'permissions.descriptions.management.users',
                    'permissions.descriptions.management.teams',
                    'permissions.descriptions.management.profiles'
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE Permissions SET [Active] = 1
                WHERE [Description] IN (
                    'permissions.descriptions.quizzes',
                    'permissions.descriptions.management',
                    'permissions.descriptions.users',
                    'permissions.descriptions.teams',
                    'permissions.descriptions.profiles',
                    'permissions.descriptions.tools'
                );
            ");
        }
    }
}
