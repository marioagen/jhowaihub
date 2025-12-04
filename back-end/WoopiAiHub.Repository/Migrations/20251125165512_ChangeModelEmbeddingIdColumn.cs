using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModelEmbeddingIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsageDailies_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageDailies");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageLogs_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ModelEmbeddingId",
                table: "UsageLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ModelEmbeddingId",
                table: "UsageDailies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageDailies_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageDailies",
                column: "ModelEmbeddingId",
                principalTable: "ModelEmbeddings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsageLogs_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageLogs",
                column: "ModelEmbeddingId",
                principalTable: "ModelEmbeddings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsageDailies_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageDailies");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageLogs_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ModelEmbeddingId",
                table: "UsageLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ModelEmbeddingId",
                table: "UsageDailies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageDailies_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageDailies",
                column: "ModelEmbeddingId",
                principalTable: "ModelEmbeddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageLogs_ModelEmbeddings_ModelEmbeddingId",
                table: "UsageLogs",
                column: "ModelEmbeddingId",
                principalTable: "ModelEmbeddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
