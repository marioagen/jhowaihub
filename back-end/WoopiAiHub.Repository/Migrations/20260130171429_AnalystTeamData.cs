using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AnalystTeamData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Teams WHERE [Name] = 'Analyst')
                BEGIN
                   INSERT INTO Teams (Name, Created)
                   VALUES ('Analyst', GETDATE());
                END

            
                IF NOT EXISTS (SELECT 1 FROM Profiles WHERE [Name] = 'Analyst')
                BEGIN
                   INSERT INTO Profiles (Name, Created)
                   VALUES ('Analyst', GETDATE());
                END
            ");

            migrationBuilder.Sql(@"            
               DECLARE @ProfileIdAnalyst INT;
               DECLARE @TeamIdAnalyst INT;
               DECLARE @DocumentsPermissionId INT;
               DECLARE @WorkflowPermissionId INT;

               
               SELECT @ProfileIdAnalyst = Id FROM Profiles WHERE [Name] = 'Analyst';
               SELECT @TeamIdAnalyst = Id FROM Teams WHERE [Name] = 'Analyst';

               Select  @DocumentsPermissionId = Id FROM Permissions WHERE [Group] = 'Documents';
               Select  @WorkflowPermissionId = Id FROM Permissions WHERE [Group] = 'Workflow';    


               INSERT INTO ProfilePermissions (PermissionId, ProfileId)
               VALUES 
                (@DocumentsPermissionId, @ProfileIdAnalyst),
                (@WorkflowPermissionId, @ProfileIdAnalyst);
            
               INSERT INTO TeamProfiles (TeamId, ProfileId)
               VALUES 
                (@TeamIdAnalyst, @ProfileIdAnalyst);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("");
        }
    }
}
