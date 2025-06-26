using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations.applicationDB
{
    /// <inheritdoc />
    public partial class addemailcreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailCreator",
                table: "TypeDoc",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailCreator",
                table: "Questions",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailCreator",
                table: "Questionnaires",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCreator",
                table: "TypeDoc");

            migrationBuilder.DropColumn(
                name: "EmailCreator",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "EmailCreator",
                table: "Questionnaires");
        }
    }
}
