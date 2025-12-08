using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddStepProfilePermissionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Workflows");

            migrationBuilder.CreateTable(
                name: "StepProfilePermissions",
                columns: table => new
                {
                    StepId = table.Column<int>(type: "int", nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepProfilePermissions", x => new { x.StepId, x.ProfileId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_StepProfilePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepProfilePermissions_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepProfilePermissions_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamProfiles",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamProfiles", x => new { x.TeamId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_TeamProfiles_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamProfiles_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StepProfilePermissions_PermissionId",
                table: "StepProfilePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_StepProfilePermissions_ProfileId",
                table: "StepProfilePermissions",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamProfiles_ProfileId",
                table: "TeamProfiles",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StepProfilePermissions");

            migrationBuilder.DropTable(
                name: "TeamProfiles");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Workflows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows",
                column: "TeamId");
        }
    }
}
