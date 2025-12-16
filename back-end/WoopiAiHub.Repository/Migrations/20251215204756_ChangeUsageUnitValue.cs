using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUsageUnitValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "UsageUnits",
                type: "decimal(18,7)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "UsageUnits",
                type: "decimal(5,5)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,7)");
        }
    }
}
