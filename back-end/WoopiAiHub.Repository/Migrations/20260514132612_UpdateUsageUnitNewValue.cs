using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsageUnitNewValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE u
                SET u.Value = 0.000000790
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
