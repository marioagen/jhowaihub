using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddModelGpt41 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               IF NOT EXISTS (SELECT 1 FROM ModelEmbeddings WHERE [Name] = 'gpt-4.1')
               BEGIN
                   INSERT INTO ModelEmbeddings (Name, Created)
                   VALUES ('gpt-4.1', GETDATE());
               END
           ");

            migrationBuilder.Sql(@"
               DECLARE @EmbeddingIdGpt41 INT;
               DECLARE @UsageTypeIdToken INT;

               SELECT @EmbeddingIdGpt41 = Id FROM ModelEmbeddings WHERE [Name] = 'gpt-4.1';
               SELECT @UsageTypeIdToken = Id FROM UsageTypes WHERE [Name] = 'Token';

               IF NOT EXISTS (SELECT 1 FROM UsageUnits WHERE [UsageTypeId] = @UsageTypeIdToken AND [ModelEmbeddingId] = @EmbeddingIdGpt41)
               BEGIN
                   INSERT INTO UsageUnits (Name, UsageTypeId, ModelEmbeddingId, Value, Created)
                   VALUES ('Unit', @UsageTypeIdToken, @EmbeddingIdGpt41, 0.000000790,  GETDATE());
               END
           ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
