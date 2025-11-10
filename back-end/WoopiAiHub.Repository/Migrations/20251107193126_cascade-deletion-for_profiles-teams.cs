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

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Documents_DocumentId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_DocumentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Teams");
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

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DocumentId",
                table: "Teams",
                column: "DocumentId");

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
