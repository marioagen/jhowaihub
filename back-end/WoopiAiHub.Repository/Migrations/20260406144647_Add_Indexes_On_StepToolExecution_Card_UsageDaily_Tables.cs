using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indexes_On_StepToolExecution_Card_UsageDaily_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StepToolExecutions_StepToolId",
                table: "StepToolExecutions");

            migrationBuilder.DropIndex(
                name: "IX_Cards_DocumentBatchId",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_UsageDaily_Processed_UsageTypeId",
                table: "UsageDailies",
                columns: new[] { "Processed", "UsageTypeId" })
                .Annotation("SqlServer:Online", true);

            migrationBuilder.CreateIndex(
                name: "IX_StepToolExecution_StepToolId_CardId",
                table: "StepToolExecutions",
                columns: new[] { "StepToolId", "CardId" })
                .Annotation("SqlServer:Online", true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DocumentBatchId",
                table: "Cards",
                column: "DocumentBatchId",
                filter: "[DocumentBatchId] IS NOT NULL")
                .Annotation("SqlServer:Online", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsageDaily_Processed_UsageTypeId",
                table: "UsageDailies");

            migrationBuilder.DropIndex(
                name: "IX_StepToolExecution_StepToolId_CardId",
                table: "StepToolExecutions");

            migrationBuilder.DropIndex(
                name: "IX_Cards_DocumentBatchId",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_StepToolExecutions_StepToolId",
                table: "StepToolExecutions",
                column: "StepToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DocumentBatchId",
                table: "Cards",
                column: "DocumentBatchId");
        }
    }
}
