using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCascadeStepTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StepToolExecutions_StepTools_StepToolId",
                table: "StepToolExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_StepToolOutputs_StepTools_StepToolId",
                table: "StepToolOutputs");

            migrationBuilder.DropForeignKey(
                name: "FK_StepToolParameters_StepTools_StepToolId",
                table: "StepToolParameters");

            migrationBuilder.DropForeignKey(
                name: "FK_StepTools_Steps_StepId",
                table: "StepTools");

            migrationBuilder.AddForeignKey(
                name: "FK_StepToolExecutions_StepTools_StepToolId",
                table: "StepToolExecutions",
                column: "StepToolId",
                principalTable: "StepTools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StepToolOutputs_StepTools_StepToolId",
                table: "StepToolOutputs",
                column: "StepToolId",
                principalTable: "StepTools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StepToolParameters_StepTools_StepToolId",
                table: "StepToolParameters",
                column: "StepToolId",
                principalTable: "StepTools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StepTools_Steps_StepId",
                table: "StepTools",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StepToolExecutions_StepTools_StepToolId",
                table: "StepToolExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_StepToolOutputs_StepTools_StepToolId",
                table: "StepToolOutputs");

            migrationBuilder.DropForeignKey(
                name: "FK_StepToolParameters_StepTools_StepToolId",
                table: "StepToolParameters");

            migrationBuilder.DropForeignKey(
                name: "FK_StepTools_Steps_StepId",
                table: "StepTools");

            migrationBuilder.AddForeignKey(
                name: "FK_StepToolExecutions_StepTools_StepToolId",
                table: "StepToolExecutions",
                column: "StepToolId",
                principalTable: "StepTools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StepToolOutputs_StepTools_StepToolId",
                table: "StepToolOutputs",
                column: "StepToolId",
                principalTable: "StepTools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StepToolParameters_StepTools_StepToolId",
                table: "StepToolParameters",
                column: "StepToolId",
                principalTable: "StepTools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StepTools_Steps_StepId",
                table: "StepTools",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
