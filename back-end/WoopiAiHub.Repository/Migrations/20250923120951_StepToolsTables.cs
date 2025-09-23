using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class StepToolsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StepTools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    DependsOnStepToolId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepTools_StepTools_DependsOnStepToolId",
                        column: x => x.DependsOnStepToolId,
                        principalTable: "StepTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepTools_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepTools_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StepToolExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepToolId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Started = table.Column<DateTime>(type: "datetime", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepToolExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepToolExecutions_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepToolExecutions_StepTools_StepToolId",
                        column: x => x.StepToolId,
                        principalTable: "StepTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StepToolOutputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepToolId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepToolOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepToolOutputs_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepToolOutputs_StepTools_StepToolId",
                        column: x => x.StepToolId,
                        principalTable: "StepTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StepToolParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepToolId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepToolParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepToolParameters_StepTools_StepToolId",
                        column: x => x.StepToolId,
                        principalTable: "StepTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StepToolExecutions_CardId",
                table: "StepToolExecutions",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_StepToolExecutions_StepToolId",
                table: "StepToolExecutions",
                column: "StepToolId");

            migrationBuilder.CreateIndex(
                name: "IX_StepToolOutputs_CardId",
                table: "StepToolOutputs",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_StepToolOutputs_StepToolId",
                table: "StepToolOutputs",
                column: "StepToolId");

            migrationBuilder.CreateIndex(
                name: "IX_StepToolParameters_StepToolId",
                table: "StepToolParameters",
                column: "StepToolId");

            migrationBuilder.CreateIndex(
                name: "IX_StepTools_DependsOnStepToolId",
                table: "StepTools",
                column: "DependsOnStepToolId",
                unique: true,
                filter: "[DependsOnStepToolId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StepTools_StepId",
                table: "StepTools",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_StepTools_ToolId",
                table: "StepTools",
                column: "ToolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StepToolExecutions");

            migrationBuilder.DropTable(
                name: "StepToolOutputs");

            migrationBuilder.DropTable(
                name: "StepToolParameters");

            migrationBuilder.DropTable(
                name: "StepTools");
        }
    }
}
