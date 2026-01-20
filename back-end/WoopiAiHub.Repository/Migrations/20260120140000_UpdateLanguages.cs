using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                WHERE [Group] = 'Prompts';
                

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.questions'
                WHERE Description = 'Tela de questões';

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.types'
                WHERE Description = 'Tela de tipos';

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.quizzes'
                WHERE Description = 'Tela de questionários';

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.documents'
                WHERE Description = 'Tela de documentos';

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.management'
                WHERE Description = 'Tela de gestão';

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.users'
                WHERE Description = 'Tela de usuários';

                UPDATE Permissions 
                SET Description = 'permissions.descriptions.teams'
                WHERE Description = 'Tela de times';

                UPDATE Permissions
                SET Description = 'permissions.descriptions.profiles'
                WHERE Description = 'Tela de perfis';

                UPDATE Permissions
                SET Description = 'permissions.descriptions.workflow'
                WHERE Description = 'Tela de workflows';

                UPDATE Permissions
                SET Description = 'permissions.descriptions.tools'
                WHERE Description = 'Tela de ferramentas';

                UPDATE Permissions
                SET Description = 'permissions.descriptions.workflowStepView'
                WHERE Description = 'Somente visualizar';

                UPDATE Permissions
                SET Description = 'permissions.descriptions.workflowStepAccess'
                WHERE Description = 'Acessar';

                UPDATE Permissions  
                SET Description = 'permissions.descriptions.dashboard'
                WHERE Description = 'Tela de dashboard';
                
                UPDATE Permissions
                SET Description = 'permissions.descriptions.prompts'
                WHERE Description = 'Tela de prompts';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
