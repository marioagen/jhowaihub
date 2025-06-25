using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocAnalyzer.Repository.Migrations.applicationDB
{
    /// <inheritdoc />
    public partial class changedocumentnamesize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                type: "varchar(251)",
                maxLength: 251,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(251)",
                oldMaxLength: 251);
        }
    }
}
