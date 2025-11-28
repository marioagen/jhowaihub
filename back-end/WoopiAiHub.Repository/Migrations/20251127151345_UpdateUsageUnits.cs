using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsageUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_UsageUnits_UsageTypes_UsageTypeId",
                table: "UsageUnits");

            migrationBuilder.AlterColumn<int>(
                name: "UsageTypeId",
                table: "UsageUnits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ModelEmbeddingId",
                table: "UsageUnits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageUnits_ModelEmbeddingId",
                table: "UsageUnits",
                column: "ModelEmbeddingId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageUnits_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageUnits",
                column: "ModelEmbeddingId",
                principalTable: "ModelEmbeddings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageUnits_UsageTypes_UsageTypeId",
                table: "UsageUnits",
                column: "UsageTypeId",
                principalTable: "UsageTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsageUnits_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageUnits_UsageTypes_UsageTypeId",
                table: "UsageUnits");

            migrationBuilder.DropIndex(
                name: "IX_UsageUnits_ModelEmbeddingId",
                table: "UsageUnits");

            migrationBuilder.DropColumn(
                name: "ModelEmbeddingId",
                table: "UsageUnits");

            migrationBuilder.AlterColumn<int>(
                name: "UsageTypeId",
                table: "UsageUnits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageUnits_UsageTypes_UsageTypeId",
                table: "UsageUnits",
                column: "UsageTypeId",
                principalTable: "UsageTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
