using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWrkGroupsTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				UPDATE Permissions 
				SET [Group] = 'Questions'
				WHERE [Group] = 'permissions.groups.questions';

				UPDATE Permissions 
				SET [Group] = 'Types'
				WHERE [Group] = 'permissions.groups.types';

				UPDATE Permissions 
				SET [Group] = 'Quizzes'
				WHERE [Group] = 'permissions.groups.quizzes';

				UPDATE Permissions 
				SET [Group] = 'Documents'
				WHERE [Group] = 'permissions.groups.documents';

				UPDATE Permissions 
				SET [Group] = 'Management'
				WHERE [Group] = 'permissions.groups.management';

				UPDATE Permissions 
				SET [Group] = 'Users'
				WHERE [Group] = 'permissions.groups.users';

				UPDATE Permissions 
				SET [Group] = 'Teams'
				WHERE [Group] = 'permissions.groups.teams';

				UPDATE Permissions
				SET [Group] = 'Profiles'
				WHERE [Group] = 'permissions.groups.profiles';

				UPDATE Permissions
				SET [Group] = 'Workflow'
				WHERE [Group] = 'permissions.groups.workflow';

				UPDATE Permissions
				SET [Group] = 'Tools'
				WHERE [Group] = 'permissions.groups.tools';

				UPDATE Permissions
				SET [Group] = 'Workflow-Step'
				WHERE [Group] = 'permissions.groups.workflowStep';

				UPDATE Permissions
				SET [Group] = 'Dashboard'
				WHERE [Group] = 'permissions.groups.dashboard';

				UPDATE Permissions
				SET [Group] = 'Prompts'
				WHERE [Group] = 'permissions.groups.prompts';

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
