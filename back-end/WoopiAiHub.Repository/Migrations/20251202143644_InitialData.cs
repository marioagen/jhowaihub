using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
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

            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM ModelEmbeddings WHERE [Name] = 'gpt-4o')
               BEGIN
                   INSERT INTO ModelEmbeddings (Name, Created)
                   VALUES ('gpt-4o', GETDATE());
               END
           
               IF NOT EXISTS (SELECT 1 FROM ModelEmbeddings WHERE [Name] = 'text-embedding-3-large')
               BEGIN
                   INSERT INTO ModelEmbeddings (Name, Created)
                   VALUES ('text-embedding-3-large', GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM ToolDatas WHERE [Name] = 'PDF')
               BEGIN
                   INSERT INTO ToolDatas (Name, IsActive, Created)
                   VALUES ('PDF', 1, GETDATE());
               END
           
               IF NOT EXISTS (SELECT 1 FROM ToolDatas WHERE [Name] = 'Texto')
               BEGIN
                   INSERT INTO ToolDatas (Name, IsActive, Created)
                   VALUES ('Texto', 1, GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM ToolTypes WHERE [Name] = 'OCR')
               BEGIN
                   INSERT INTO ToolTypes (Name, IsActive, Created)
                   VALUES ('OCR', 1, GETDATE());
               END           

               IF NOT EXISTS (SELECT 1 FROM ToolTypes WHERE [Name] = 'Embeddings')
               BEGIN
                   INSERT INTO ToolTypes (Name, IsActive, Created)
                   VALUES ('Embeddings', 1, GETDATE());
               END
           
               IF NOT EXISTS (SELECT 1 FROM ToolTypes WHERE [Name] = 'Prompt')
               BEGIN
                   INSERT INTO ToolTypes (Name, IsActive, Created)
                   VALUES ('Prompt', 1, GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM ToolTypes WHERE [Name] = 'N8N')
               BEGIN
                   INSERT INTO ToolTypes (Name, IsActive, Created)
                   VALUES ('N8N', 1, GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               DECLARE @ToolDataIdPdf INT;
               DECLARE @ToolDataIdTexto INT;
               DECLARE @ToolTypeIdOcr INT;
               DECLARE @ToolTypeIdPrompt INT;
               DECLARE @ToolTypeIdEmbeddings INT;

               SELECT @ToolDataIdPdf = Id FROM ToolDatas WHERE [Name] = 'PDF';
               SELECT @ToolDataIdTexto = Id FROM ToolDatas WHERE [Name] = 'Texto';
               SELECT @ToolTypeIdOcr = Id FROM ToolTypes WHERE [Name] = 'OCR';
               SELECT @ToolTypeIdPrompt = Id FROM ToolTypes WHERE [Name] = 'Prompt';
               SELECT @ToolTypeIdEmbeddings = Id FROM ToolTypes WHERE [Name] = 'Embeddings';

               IF NOT EXISTS (SELECT 1 FROM Tools WHERE [Name] = 'OCR')
               BEGIN
                   INSERT INTO Tools (Name, IsActive, ToolTypeId, InputDataId, OutputDataId, Created)
                   VALUES ('Ocr', 1, @ToolTypeIdOcr, @ToolDataIdPdf, @ToolDataIdTexto,  GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM Tools WHERE [Name] = 'Embeddings')
               BEGIN
                   INSERT INTO Tools (Name, IsActive, ToolTypeId, InputDataId, OutputDataId, Created)
                   VALUES ('Embeddings', 1, @ToolTypeIdEmbeddings, @ToolDataIdTexto, @ToolDataIdTexto,  GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM Tools WHERE [Name] = 'Prompt')
               BEGIN
                   INSERT INTO Tools (Name, IsActive, ToolTypeId, InputDataId, OutputDataId, Created, IsEditableInput)
                   VALUES ('Prompt', 1, @ToolTypeIdPrompt, @ToolDataIdTexto, @ToolDataIdTexto,  GETDATE(), 1);
               END
           ");

            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM UsageTypes WHERE [Name] = 'Page')
               BEGIN
                   INSERT INTO UsageTypes (Name,Created)
                   VALUES ('Page', GETDATE());
               END           

               IF NOT EXISTS (SELECT 1 FROM UsageTypes WHERE [Name] = 'Automation')
               BEGIN
                   INSERT INTO UsageTypes (Name,Created)
                   VALUES ('Automation', GETDATE());
               END
           
               IF NOT EXISTS (SELECT 1 FROM UsageTypes WHERE [Name] = 'Execution')
               BEGIN
                   INSERT INTO UsageTypes (Name, Created)
                   VALUES ('Execution', GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM UsageTypes WHERE [Name] = 'Token')
               BEGIN
                   INSERT INTO UsageTypes (Name, Created)
                   VALUES ('Token', GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               DECLARE @EmbeddingIdGpt4o INT;
               DECLARE @EmbeddingIdTextEmbedding3Large INT;
               DECLARE @UsageTypeIdPage INT;
               DECLARE @UsageTypeIdAutomation INT;
               DECLARE @UsageTypeIdExecution INT;
               DECLARE @UsageTypeIdToken INT;

               SELECT @EmbeddingIdGpt4o = Id FROM ModelEmbeddings WHERE [Name] = 'gpt-4o';
               SELECT @EmbeddingIdTextEmbedding3Large = Id FROM ModelEmbeddings WHERE [Name] = 'text-embedding-3-large';

               SELECT @UsageTypeIdPage = Id FROM UsageTypes WHERE [Name] = 'Page';
               SELECT @UsageTypeIdAutomation = Id FROM UsageTypes WHERE [Name] = 'Automation';
               SELECT @UsageTypeIdExecution = Id FROM UsageTypes WHERE [Name] = 'Execution';
               SELECT @UsageTypeIdToken = Id FROM UsageTypes WHERE [Name] = 'Token';

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdPage)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdPage, 0.00001,  GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdAutomation)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdAutomation, 0.00001,  GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdExecution)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdExecution, 0.00001,  GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdToken AND [ModelEmbeddingId] = @EmbeddingIdGpt4o)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, ModelEmbeddingId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdToken, @EmbeddingIdGpt4o, 0.0001,  GETDATE());
               END

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdToken AND [ModelEmbeddingId] = @EmbeddingIdTextEmbedding3Large)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, ModelEmbeddingId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdToken, @EmbeddingIdTextEmbedding3Large, 0.0001,  GETDATE());
               END
           ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
 
        }
    }
}
