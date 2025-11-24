using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentIdTableTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remover FK se existir
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_Teams_Documents_DocumentId'
                      AND parent_object_id = OBJECT_ID('Teams')
                )
                BEGIN
                    ALTER TABLE Teams DROP CONSTRAINT FK_Teams_Documents_DocumentId;
                END
            ");

            // Remover índice se existir
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Teams_DocumentId'
                      AND object_id = OBJECT_ID('Teams')
                )
                BEGIN
                    DROP INDEX IX_Teams_DocumentId ON Teams;
                END
            ");

            // Remover a coluna se existir
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = 'DocumentId'
                      AND Object_ID = Object_ID('Teams')
                )
                BEGIN
                    ALTER TABLE Teams DROP COLUMN DocumentId;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Adicionar a coluna de volta caso não exista
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = 'DocumentId'
                      AND Object_ID = Object_ID('Teams')
                )
                BEGIN
                    ALTER TABLE Teams ADD DocumentId int NULL;
                END
            ");

            // Adicionar índice de volta caso não exista
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Teams_DocumentId'
                      AND object_id = OBJECT_ID('Teams')
                )
                BEGIN
                    CREATE INDEX IX_Teams_DocumentId ON Teams(DocumentId);
                END
            ");

            // Adicionar FK de volta caso não exista
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_Teams_Documents_DocumentId'
                      AND parent_object_id = OBJECT_ID('Teams')
                )
                BEGIN
                    ALTER TABLE Teams
                    ADD CONSTRAINT FK_Teams_Documents_DocumentId
                        FOREIGN KEY (DocumentId) REFERENCES Documents(Id);
                END
            ");
        }
    }
}
