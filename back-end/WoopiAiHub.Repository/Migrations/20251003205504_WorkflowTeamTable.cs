using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowTeamTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Teams_TeamId",
                table: "Workflows");

            migrationBuilder.DropTable(
                name: "DocumentTeams");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowTeams",
                columns: table => new
                {
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTeams", x => new { x.WorkflowId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_WorkflowTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowTeams_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DocumentId",
                table: "Teams",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTeams_TeamId",
                table: "WorkflowTeams",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Documents_DocumentId",
                table: "Teams",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Documents_DocumentId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "WorkflowTeams");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Teams_DocumentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Teams");

            migrationBuilder.CreateTable(
                name: "DocumentTeams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTeams", x => new { x.TeamId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_DocumentTeams_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTeams_DocumentId",
                table: "DocumentTeams",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Teams_TeamId",
                table: "Workflows",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
