using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Addn8nColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectorApiKey",
                table: "Tools",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectorUrl",
                table: "Tools",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiredFile",
                table: "StepToolParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "WebhookId",
                table: "StepToolParameters",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectorApiKey",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "ConnectorUrl",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "RequiredFile",
                table: "StepToolParameters");

            migrationBuilder.DropColumn(
                name: "WebhookId",
                table: "StepToolParameters");
        }
    }
}
