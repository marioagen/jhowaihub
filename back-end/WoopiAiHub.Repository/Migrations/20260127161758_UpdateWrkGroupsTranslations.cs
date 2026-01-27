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

					migrationBuilder.Sql(@"
                UPDATE Permissions 
                SET [Group] = 'permissions.groups.questions'
                WHERE [Group] = 'Questions';

                UPDATE Permissions 
                SET [Group] = 'permissions.groups.types'
                WHERE [Group] = 'Types';

                UPDATE Permissions 
                SET [Group] = 'permissions.groups.quizzes'
                WHERE [Group] = 'Quizzes';

                UPDATE Permissions 
                SET [Group] = 'permissions.groups.documents'
                WHERE [Group] = 'Documents';

                UPDATE Permissions 
                SET [Group] = 'permissions.groups.management'
                WHERE [Group] = 'Management';

                UPDATE Permissions 
                SET [Group] = 'permissions.groups.users'
                WHERE [Group] = 'Users';

                UPDATE Permissions 
                SET [Group] = 'permissions.groups.teams'
                WHERE [Group] = 'Teams';

                UPDATE Permissions
                SET [Group] = 'permissions.groups.profiles'
                WHERE [Group] = 'Profiles';

                UPDATE Permissions
                SET [Group] = 'permissions.groups.workflow'
                WHERE [Group] = 'Workflow';

                UPDATE Permissions
                SET [Group] = 'permissions.groups.tools'
                WHERE [Group] = 'Tools';

                UPDATE Permissions
                SET [Group] = 'permissions.groups.workflowStep'
                WHERE [Group] = 'Workflow-Step';
                
                UPDATE Permissions
                SET [Group] = 'permissions.groups.dashboard'
                WHERE [Group] = 'Dashboard';

                UPDATE Permissions
                SET [Group] = 'permissions.groups.prompts'
                WHERE [Group] = 'Prompts';");
        }
    }
}
