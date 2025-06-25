using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocAnalyzer.Repository.Migrations.applicationDB
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nr_IDEA = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    Reference_File = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Enable = table.Column<bool>(type: "bit", nullable: false),
                    EmailCreator = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InquiryHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Inquiry = table.Column<int>(type: "int", nullable: false),
                    Input = table.Column<string>(type: "varchar(max)", nullable: false),
                    Output = table.Column<string>(type: "varchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InquiryHistories_Inquiries_Id_Inquiry",
                        column: x => x.Id_Inquiry,
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InquiryNormalized",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Inquiry = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "varchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryNormalized", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InquiryNormalized_Inquiries_Id_Inquiry",
                        column: x => x.Id_Inquiry,
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InquiryHistories_Id_Inquiry",
                table: "InquiryHistories",
                column: "Id_Inquiry");

            migrationBuilder.CreateIndex(
                name: "IX_InquiryNormalized_Id_Inquiry",
                table: "InquiryNormalized",
                column: "Id_Inquiry",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InquiryHistories");

            migrationBuilder.DropTable(
                name: "InquiryNormalized");

            migrationBuilder.DropTable(
                name: "Inquiries");
        }
    }
}
