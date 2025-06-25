using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocAnalyzer.Repository.Migrations.applicationDB
{
    /// <inheritdoc />
    public partial class changeinquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "InquiryHistories",
                newName: "DocumentHistories"
            );

            migrationBuilder.RenameTable(
                name: "InquiryNormalized",
                newName: "DocumentNormalized"
            );

            migrationBuilder.RenameTable(
                name: "Inquiries",
                newName: "Documents"
            );

            migrationBuilder.RenameColumn(
               name: "Id_Inquiry",
               table: "DocumentHistories",
               newName: "Id_Document");

            migrationBuilder.RenameColumn(
              name: "Id_Inquiry",
              table: "DocumentNormalized",
              newName: "Id_Document");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
               name: "DocumentHistories",
               newName: "InquiryHistories"
           );

            migrationBuilder.RenameTable(
                name: "DocumentNormalized",
                newName: "InquiryNormalized"
            );

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Inquiries"
            );

            migrationBuilder.RenameColumn(
               name: "Id_Document",
               table: "InquiryHistories",
               newName: "Id_Inquiry");

            migrationBuilder.RenameColumn(
              name: "Id_Document",
              table: "InquiryNormalized",
              newName: "Id_Inquiry");
        }
    }
}
