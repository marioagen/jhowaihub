using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Status",
                type: "varchar(7)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Status SET Color = '#D97706' WHERE Id = 1;");
            migrationBuilder.Sql("UPDATE Status SET Color = '#2563EB' WHERE Id = 2;");
            migrationBuilder.Sql("UPDATE Status SET Color = '#EA580C' WHERE Id = 3;");
            migrationBuilder.Sql("UPDATE Status SET Color = '#16A34A' WHERE Id = 4;");
            migrationBuilder.Sql("UPDATE Status SET Color = '#4B5563' WHERE Id = 5;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Status");
        }
    }
}
