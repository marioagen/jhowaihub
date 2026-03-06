using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AuditCardTable : Migration
    {
        private const string AuditCardsTableName = "AuditCards";
        private const string EnableColumnName = "Enable";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: AuditCardsTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditCards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditCards_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditCards_ActionType",
                table: AuditCardsTableName,
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditCards_CardId",
                table: AuditCardsTableName,
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditCards_OccurredAt",
                table: AuditCardsTableName,
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditCards_UserId",
                table: AuditCardsTableName,
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditCards_WorkflowId",
                table: AuditCardsTableName,
                column: "WorkflowId");

            migrationBuilder.AddColumn<bool>(
                name: EnableColumnName,
                table: "Cards",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: EnableColumnName,
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: AuditCardsTableName);

            migrationBuilder.DropColumn(
                name: EnableColumnName,
                table: "Cards");

            migrationBuilder.DropColumn(
                name: EnableColumnName,
                table: "Documents");
        }
    }
}
