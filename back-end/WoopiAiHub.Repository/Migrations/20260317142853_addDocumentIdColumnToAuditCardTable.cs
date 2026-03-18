using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addDocumentIdColumnToAuditCardTable : Migration
    {
        private const string AuditCardsTableName = "AuditCards";
        private const string DocumentIdColumnName = "DocumentId";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: DocumentIdColumnName,
                table: AuditCardsTableName,
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                $"UPDATE {AuditCardsTableName} SET {DocumentIdColumnName} = (SELECT DocumentId FROM Cards WHERE Cards.Id = {AuditCardsTableName}.CardId);");

            migrationBuilder.AlterColumn<int>(
                name: DocumentIdColumnName,
                table: AuditCardsTableName,
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: $"IX_{AuditCardsTableName}_{DocumentIdColumnName}",
                table: AuditCardsTableName,
                column: DocumentIdColumnName);

            migrationBuilder.AddForeignKey(
                name: $"FK_{AuditCardsTableName}_Documents_{DocumentIdColumnName}",
                table: AuditCardsTableName,
                column: DocumentIdColumnName,
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: $"FK_{AuditCardsTableName}_Documents_{DocumentIdColumnName}",
                table: AuditCardsTableName);

            migrationBuilder.DropIndex(
                name: $"IX_{AuditCardsTableName}_{DocumentIdColumnName}",
                table: AuditCardsTableName);

            migrationBuilder.DropColumn(
                name: DocumentIdColumnName,
                table: AuditCardsTableName);
        }
    }
}
