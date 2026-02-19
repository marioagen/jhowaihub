using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_Description_For_ToolType_Api : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysisRejections_Cards_CardId",
                table: "DocumentAnalysisRejections");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysisRejections_Steps_StepId",
                table: "DocumentAnalysisRejections");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysisRejections_Users_UserId",
                table: "DocumentAnalysisRejections");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysisRejections_Cards_CardId",
                table: "DocumentAnalysisRejections",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysisRejections_Steps_StepId",
                table: "DocumentAnalysisRejections",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysisRejections_Users_UserId",
                table: "DocumentAnalysisRejections",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("UPDATE ToolTypes SET [Description] = 'tools.typeDisplay.Api' WHERE [Name] = 'API'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysisRejections_Cards_CardId",
                table: "DocumentAnalysisRejections");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysisRejections_Steps_StepId",
                table: "DocumentAnalysisRejections");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysisRejections_Users_UserId",
                table: "DocumentAnalysisRejections");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysisRejections_Cards_CardId",
                table: "DocumentAnalysisRejections",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysisRejections_Steps_StepId",
                table: "DocumentAnalysisRejections",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysisRejections_Users_UserId",
                table: "DocumentAnalysisRejections",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
