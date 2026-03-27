using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class DocumentRejection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Prompts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(95)",
                oldMaxLength: 95);

            migrationBuilder.CreateTable(
                name: "DocumentAnalysisRejections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Justification = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAnalysisRejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAnalysisRejections_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentAnalysisRejections_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentAnalysisRejections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysisRejections_CardId",
                table: "DocumentAnalysisRejections",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysisRejections_Created",
                table: "DocumentAnalysisRejections",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysisRejections_StepId",
                table: "DocumentAnalysisRejections",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysisRejections_UserId",
                table: "DocumentAnalysisRejections",
                column: "UserId");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'DocumentRejection' AND [Description] = 'permissions.descriptions.documentRejection')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('DocumentRejection', GETDATE(), 'permissions.descriptions.documentRejection', 'Actions');
                END

                IF NOT EXISTS (SELECT 1 FROM Status WHERE [Name] = 'Rejected')
                BEGIN
                    INSERT INTO Status (Name, Color, Label, Created)
                    VALUES ('Rejected', '#FF0000', 'workflow.statusList.rejected', GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentAnalysisRejections");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Prompts",
                type: "varchar(95)",
                maxLength: 95,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.Sql(
                "DELETE FROM Permissions WHERE [Name] = 'DocumentRejection' AND [Description] = 'permissions.descriptions.documentRejection' AND [Group] = 'Actions';");

            migrationBuilder.Sql(
                "DELETE FROM Status WHERE [Name] = 'Rejected';");
        }
    }
}
