using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_WorkflowId_On_UsageDaily_And_UsageMonth_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsageDailies_UserId",
                table: "UsageDailies");

            migrationBuilder.DropIndex(
                name: "IX_UsageDaily_Processed_UsageTypeId",
                table: "UsageDailies");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowId",
                table: "UsageMonths",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowId",
                table: "UsageDailies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageMonths_WorkflowId",
                table: "UsageMonths",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageDaily_Created",
                table: "UsageDailies",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_UsageDaily_Processed_Created",
                table: "UsageDailies",
                columns: new[] { "Processed", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageDaily_UserId_Processed",
                table: "UsageDailies",
                columns: new[] { "UserId", "Processed" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageDaily_WorkflowId_Processed",
                table: "UsageDailies",
                columns: new[] { "WorkflowId", "Processed" });

            migrationBuilder.AddForeignKey(
                name: "FK_UsageDailies_Workflows_WorkflowId",
                table: "UsageDailies",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageMonths_Workflows_WorkflowId",
                table: "UsageMonths",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsageDailies_Workflows_WorkflowId",
                table: "UsageDailies");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageMonths_Workflows_WorkflowId",
                table: "UsageMonths");

            migrationBuilder.DropIndex(
                name: "IX_UsageMonths_WorkflowId",
                table: "UsageMonths");

            migrationBuilder.DropIndex(
                name: "IX_UsageDaily_Created",
                table: "UsageDailies");

            migrationBuilder.DropIndex(
                name: "IX_UsageDaily_Processed_Created",
                table: "UsageDailies");

            migrationBuilder.DropIndex(
                name: "IX_UsageDaily_UserId_Processed",
                table: "UsageDailies");

            migrationBuilder.DropIndex(
                name: "IX_UsageDaily_WorkflowId_Processed",
                table: "UsageDailies");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "UsageMonths");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "UsageDailies");

            migrationBuilder.CreateIndex(
                name: "IX_UsageDailies_UserId",
                table: "UsageDailies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageDaily_Processed_UsageTypeId",
                table: "UsageDailies",
                columns: new[] { "Processed", "UsageTypeId" });
        }
    }
}
