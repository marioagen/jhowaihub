using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddModelGemini : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM ModelEmbeddings WHERE [Name] = 'gemini-flash-latest')
               BEGIN
                   INSERT INTO ModelEmbeddings (Name, Created)
                   VALUES ('gemini-flash-latest', GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               DECLARE @EmbeddingIdGeminiFlashLatest INT;
               DECLARE @UsageTypeIdToken INT;

               SELECT @EmbeddingIdGeminiFlashLatest = Id FROM ModelEmbeddings WHERE [Name] = 'gemini-flash-latest';
               SELECT @UsageTypeIdToken = Id FROM UsageTypes WHERE [Name] = 'Token';

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdToken AND [ModelEmbeddingId] = @EmbeddingIdGeminiFlashLatest)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, ModelEmbeddingId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdToken, @EmbeddingIdGeminiFlashLatest, 0.000000790,  GETDATE());
               END
           ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
