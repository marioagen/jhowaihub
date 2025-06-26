using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations.applicationDB
{
    /// <inheritdoc />
    public partial class changedocumentkeysindices : Migration
    {
        private const string TableDocumentHistories = "DocumentHistories";
        private const string TableDocumentNormalized = "DocumentNormalized";
        private const string TableDocuments = "Documents";
        private const string ColumnIdDocument = "Id_Document";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InquiryHistories_Inquiries_Id_Inquiry",
                table: TableDocumentHistories);

            migrationBuilder.DropForeignKey(
                name: "FK_InquiryNormalized_Inquiries_Id_Inquiry",
                table: TableDocumentNormalized);

            migrationBuilder.DropPrimaryKey(
                name: "PK_InquiryNormalized",
                table: TableDocumentNormalized);

            migrationBuilder.DropPrimaryKey(
                name: "PK_InquiryHistories",
                table: TableDocumentHistories);

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inquiries",
                table: TableDocuments);

            migrationBuilder.DropIndex(
                name: "IX_InquiryNormalized_Id_Inquiry",
                table: TableDocumentNormalized);

            migrationBuilder.DropIndex(
                name: "IX_InquiryHistories_Id_Inquiry",
                table: TableDocumentHistories);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Id_Inquiry",
                table: "DocumentHistories",
                column: ColumnIdDocument);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNormalized_Id_Inquiry",
                table: TableDocumentNormalized,
                column: ColumnIdDocument);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: TableDocuments,
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentHistories",
                table: TableDocumentHistories,
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentNormalized",
                table: TableDocumentNormalized,
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentHistories_Documents_Id_Document",
                table: TableDocumentHistories,
                column: ColumnIdDocument,
                principalTable: TableDocuments,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentNormalized_Documents_Id_Document",
                table: TableDocumentNormalized,
                column: ColumnIdDocument,
                principalTable: TableDocuments,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentHistories_Documents_Id_Document",
                table: TableDocumentHistories);

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentNormalized_Documents_Id_Document",
                table: TableDocumentNormalized);

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentHistories",
                table: TableDocumentHistories);

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentNormalized",
                table: TableDocumentNormalized);

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: TableDocuments);

            migrationBuilder.DropIndex(
                name: "IX_DocumentNormalized_Id_Inquiry",
                table: TableDocumentNormalized);

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Id_Inquiry",
                table: TableDocumentHistories);

            migrationBuilder.CreateIndex(
                name: "IX_InquiryHistories_Id_Inquiry",
                table: TableDocumentHistories,
                column: ColumnIdDocument);

            migrationBuilder.CreateIndex(
                name: "IX_InquiryNormalized_Id_Inquiry",
                table: TableDocumentNormalized,
                column: ColumnIdDocument);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InquiryNormalized",
                table: TableDocumentNormalized,
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InquiryHistories",
                table: TableDocumentHistories,
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inquiries",
                table: TableDocuments,
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InquiryHistories_Inquiries_Id_Inquiry",
                table: TableDocumentHistories,
                column: ColumnIdDocument,
                principalTable: TableDocuments,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InquiryNormalized_Inquiries_Id_Inquiry",
                table: TableDocumentNormalized,
                column: ColumnIdDocument,
                principalTable: TableDocuments,
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
