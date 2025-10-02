using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ToolsTables : Migration
    {
        private const string ToolDatasTableName = "ToolDatas";
        private const string ToolsTableName = "Tools";
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: ToolDatasTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: ToolsTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ToolTypeId = table.Column<int>(type: "int", nullable: false),
                    InputDataId = table.Column<int>(type: "int", nullable: false),
                    OutputDataId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tools_ToolDatas_InputDataId",
                        column: x => x.InputDataId,
                        principalTable: ToolDatasTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tools_ToolDatas_OutputDataId",
                        column: x => x.OutputDataId,
                        principalTable: ToolDatasTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tools_ToolTypes_ToolTypeId",
                        column: x => x.ToolTypeId,
                        principalTable: "ToolTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tools_InputDataId",
                table: ToolsTableName,
                column: "InputDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_OutputDataId",
                table: ToolsTableName,
                column: "OutputDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_ToolTypeId",
                table: ToolsTableName,
                column: "ToolTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: ToolsTableName);

            migrationBuilder.DropTable(
                name: ToolDatasTableName);

            migrationBuilder.DropTable(
                name: "ToolTypes");
        }
    }
}
