using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Teams WHERE [Name] = 'Admin')
                BEGIN
                    INSERT INTO Teams (Name, Created)
                    VALUES ('Admin', GETDATE());
                END
            ");


            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Profiles WHERE [Name] = 'IA')
                BEGIN
                    INSERT INTO Profiles (Name, Created)
                    VALUES ('IA', GETDATE());
                END
            
                IF NOT EXISTS (SELECT 1 FROM Profiles WHERE [Name] = 'Admin')
                BEGIN
                    INSERT INTO Profiles (Name, Created)
                    VALUES ('Admin', GETDATE());
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Question')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Question', 'Questions');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Type')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Type', 'Types');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Quizz')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Quizz', 'Quizzes');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Documents')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Documents', 'Documents');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Management Tables')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Management Tables', 'Management');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Users')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Users', 'Users');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Teams')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Teams', 'Teams');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Profiles')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Profiles', 'Profiles');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Workflow')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Workflow', 'Workflow');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Tools')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Tools', 'Tools');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Step')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Step', 'Workflow-Step');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'Access' AND [Description] = 'Access Step')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('Access', GETDATE(), 'Access Step', 'Workflow-Step');
                END
            
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'View' AND [Description] = 'View Dashboard')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('View', GETDATE(), 'View Dashboard', 'Dashboard');
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
