using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEntities : Migration
    {
        private const string StatusTableName = "Status";
        private const string WorkflowsTableName = "Workflows";
        private const string StepsTableName = "Steps";
        private const string CardsTableName = "Cards";
        private const string AnnotationSqlServer = "SqlServer:Identity";
        private const string DatetimeType = "datetime";
        private const string CreatedColumnName = "Created";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: StatusTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(AnnotationSqlServer, "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: DatetimeType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: WorkflowsTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(AnnotationSqlServer, "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(type: DatetimeType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflows_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: StepsTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(AnnotationSqlServer, "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: DatetimeType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Steps_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: StatusTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Steps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: WorkflowsTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: CardsTableName,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(AnnotationSqlServer, "1, 1"),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: DatetimeType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cards_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: StatusTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cards_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: StepsTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Created",
                table: CardsTableName,
                column: CreatedColumnName);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DocumentId",
                table: CardsTableName,
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: CardsTableName,
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_StatusId",
                table: CardsTableName,
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_StepId",
                table: CardsTableName,
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_Created",
                table: StatusTableName,
                column: CreatedColumnName);

            migrationBuilder.CreateIndex(
                name: "IX_Status_Name",
                table: StatusTableName,
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_Created",
                table: StepsTableName,
                column: CreatedColumnName);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_Name",
                table: StepsTableName,
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ProfileId",
                table: StepsTableName,
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_StatusId",
                table: StepsTableName,
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_WorkflowId_Order",
                table: StepsTableName,
                columns: ["WorkflowId", "Order"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_Created",
                table: WorkflowsTableName,
                column: CreatedColumnName);

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_TeamId",
                table: WorkflowsTableName,
                column: "TeamId",
                unique: true);

            migrationBuilder.InsertData(
                table: StatusTableName,
                columns: ["Id", "Name", CreatedColumnName],
                values: new object[,]
                {
                    { 1, "AwaitingAnalysis", DateTime.UtcNow },
                    { 2, "Analyzed", DateTime.UtcNow },
                    { 3, "WaitingForApproval", DateTime.UtcNow },
                    { 4, "Approved", DateTime.UtcNow },
                    { 5, "Done", DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                 table: StatusTableName,
                 keyColumn: "Id",
                 keyValues: [1, 2, 3, 4, 5]);

            migrationBuilder.DropTable(
                name: CardsTableName);

            migrationBuilder.DropTable(
                name: StepsTableName);

            migrationBuilder.DropTable(
                name: StatusTableName);

            migrationBuilder.DropTable(
                name: WorkflowsTableName);
        }
    }
}
