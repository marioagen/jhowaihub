using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addDocumentIdColumnToAuditCardTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "AuditCards",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE AuditCards SET DocumentId = (SELECT DocumentId FROM Cards WHERE Cards.Id = AuditCards.CardId);");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "AuditCards",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditCards_DocumentId",
                table: "AuditCards",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditCards_Documents_DocumentId",
                table: "AuditCards",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditCards_Documents_DocumentId",
                table: "AuditCards");

            migrationBuilder.DropIndex(
                name: "IX_AuditCards_DocumentId",
                table: "AuditCards");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "AuditCards");
        }
    }
}
