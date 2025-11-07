using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class cascadedeletionfor_profilesteams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfilePermissions_Permissions_PermissionId",
                table: "ProfilePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamProfiles_Teams_TeamId",
                table: "TeamProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfilePermissions_Permissions_PermissionId",
                table: "ProfilePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamProfiles_Teams_TeamId",
                table: "TeamProfiles",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfilePermissions_Permissions_PermissionId",
                table: "ProfilePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamProfiles_Teams_TeamId",
                table: "TeamProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfilePermissions_Permissions_PermissionId",
                table: "ProfilePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamProfiles_Teams_TeamId",
                table: "TeamProfiles",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
