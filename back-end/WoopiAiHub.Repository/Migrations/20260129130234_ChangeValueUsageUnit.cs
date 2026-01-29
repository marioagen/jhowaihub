using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeValueUsageUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE UsageUnits
                SET Value = 0.0082873
                WHERE UsageTypeId = (
                SELECT Id FROM UsageTypes WHERE Name = 'Page'
                );

                UPDATE UsageUnits
                SET Value = 0.3867403
                WHERE UsageTypeId = (
                SELECT Id FROM UsageTypes WHERE Name = 'Automation'
                );

                UPDATE UsageUnits
                SET Value = 0.3867403
                WHERE UsageTypeId = (
                SELECT Id FROM UsageTypes WHERE Name = 'Execution'
                );

                UPDATE u
                SET u.Value = 0.0000552
                FROM UsageUnits u
                JOIN UsageTypes t ON t.Id = u.UsageTypeId
                JOIN ModelEmbeddings m  ON m.Id = u.ModelEmbeddingId
                WHERE t.Name = 'Token'
                AND m.Name = 'gpt-4o';

                UPDATE u
                SET u.Value = 0.0007901
                FROM UsageUnits u
                JOIN UsageTypes t ON t.Id = u.UsageTypeId
                JOIN ModelEmbeddings m  ON m.Id = u.ModelEmbeddingId
                WHERE t.Name = 'Token'
                AND m.Name = 'text-embedding-3-large';
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
