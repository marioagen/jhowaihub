using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelEmbeddings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(150)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameTenant = table.Column<string>(type: "varchar(150)", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime", nullable: false),
                    TotalUsage = table.Column<int>(type: "int", nullable: false),
                    NameType = table.Column<string>(type: "varchar(100)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageDailies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsageTypeId = table.Column<int>(type: "int", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    ModelEmbeddingId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageDailies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageDailies_ModelEmbeddings_ModelEmbeddingId",
                        column: x => x.ModelEmbeddingId,
                        principalTable: "ModelEmbeddings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsageDailies_UsageTypes_UsageTypeId",
                        column: x => x.UsageTypeId,
                        principalTable: "UsageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usageMonths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsageTypeId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false),
                    ModelEmbeddingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usageMonths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usageMonths_ModelEmbeddings_ModelEmbeddingId",
                        column: x => x.ModelEmbeddingId,
                        principalTable: "ModelEmbeddings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usageMonths_UsageTypes_UsageTypeId",
                        column: x => x.UsageTypeId,
                        principalTable: "UsageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsageUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    UsageTypeId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageUnits_UsageTypes_UsageTypeId",
                        column: x => x.UsageTypeId,
                        principalTable: "UsageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsageDailies_ModelEmbeddingId",
                table: "UsageDailies",
                column: "ModelEmbeddingId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageDailies_UsageTypeId",
                table: "UsageDailies",
                column: "UsageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_usageMonths_ModelEmbeddingId",
                table: "usageMonths",
                column: "ModelEmbeddingId");

            migrationBuilder.CreateIndex(
                name: "IX_usageMonths_UsageTypeId",
                table: "usageMonths",
                column: "UsageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageUnits_UsageTypeId",
                table: "UsageUnits",
                column: "UsageTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsageDailies");

            migrationBuilder.DropTable(
                name: "UsageLogs");

            migrationBuilder.DropTable(
                name: "usageMonths");

            migrationBuilder.DropTable(
                name: "UsageUnits");

            migrationBuilder.DropTable(
                name: "ModelEmbeddings");

            migrationBuilder.DropTable(
                name: "UsageTypes");
        }
    }
}
