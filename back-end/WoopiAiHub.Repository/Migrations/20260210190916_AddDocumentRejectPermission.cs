using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentRejectPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Prompts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(95)",
                oldMaxLength: 95);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE [Name] = 'Action' AND [Description] = 'permissions.descriptions.documentReject')
                BEGIN
                    INSERT INTO Permissions (Name, Created, Description, [Group])
                    VALUES ('Action', GETDATE(), 'permissions.descriptions.documentReject', 'Documents');
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Prompts",
                type: "varchar(95)",
                maxLength: 95,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.Sql(
                "DELETE FROM Permissions WHERE [Name] = 'Action' AND [Description] = 'permissions.descriptions.documentReject' AND [Group] = 'Documents';");
        }
    }
}
