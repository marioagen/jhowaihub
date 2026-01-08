using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddApiTemplatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(GETDATE())"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Method = table.Column<string>(type: "varchar(10)", nullable: false),
                    Url = table.Column<string>(type: "varchar(100)", nullable: false),
                    QueryTemplate = table.Column<string>(type: "varchar(max)", nullable: true),
                    HeaderTemplate = table.Column<string>(type: "varchar(max)", nullable: true),
                    BodyTemplate = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiTemplates");
        }
    }
}
