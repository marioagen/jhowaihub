using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddModelEmbebddingValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM ModelEmbeddings WHERE [Name] = 'deepseek-r1')
               BEGIN
                   INSERT INTO ModelEmbeddings (Name, Created)
                   VALUES ('deepseek-r1', GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               DECLARE @EmbeddingIdDeepSeek INT;
               DECLARE @UsageTypeIdToken INT;

               SELECT @EmbeddingIdDeepSeek = Id FROM ModelEmbeddings WHERE [Name] = 'deepseek-r1';
               SELECT @UsageTypeIdToken = Id FROM UsageTypes WHERE [Name] = 'Token';

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdToken AND [ModelEmbeddingId] = @EmbeddingIdDeepSeek)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, ModelEmbeddingId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdToken, @EmbeddingIdDeepSeek, 0.000000790,  GETDATE());
               END
           ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
