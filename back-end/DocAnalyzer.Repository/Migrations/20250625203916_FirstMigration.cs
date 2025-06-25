using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocAnalyzer.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(251)", maxLength: 251, nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    Reference_File = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Enable = table.Column<bool>(type: "bit", nullable: false),
                    EmailCreator = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    EmailCreator = table.Column<string>(type: "varchar(50)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeDoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    EmailCreator = table.Column<string>(type: "varchar(50)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeDoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", nullable: false),
                    Email = table.Column<string>(type: "varchar(254)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Document = table.Column<int>(type: "int", nullable: false),
                    Input = table.Column<string>(type: "varchar(max)", nullable: false),
                    Output = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentHistories_Documents_Id_Document",
                        column: x => x.Id_Document,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentNormalized",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Document = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "varchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNormalized", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentNormalized_Documents_Id_Document",
                        column: x => x.Id_Document,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questionnaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(50)", nullable: false),
                    TypeDoc_Id = table.Column<int>(type: "int", nullable: false),
                    EmailCreator = table.Column<string>(type: "varchar(50)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questionnaires_TypeDoc_TypeDoc_Id",
                        column: x => x.TypeDoc_Id,
                        principalTable: "TypeDoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTeams",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTeams", x => new { x.UserId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_UserTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTeams_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionQuestionnaire",
                columns: table => new
                {
                    Questionnaire_Id = table.Column<int>(type: "int", nullable: false),
                    Question_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionQuestionnaire", x => new { x.Question_Id, x.Questionnaire_Id });
                    table.ForeignKey(
                        name: "FK_QuestionQuestionnaire_Questionnaires_Questionnaire_Id",
                        column: x => x.Questionnaire_Id,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionQuestionnaire_Questions_Question_Id",
                        column: x => x.Question_Id,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Id_Document",
                table: "DocumentHistories",
                column: "Id_Document");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNormalized_Id_Document",
                table: "DocumentNormalized",
                column: "Id_Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_TypeDoc_Id",
                table: "Questionnaires",
                column: "TypeDoc_Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionQuestionnaire_Questionnaire_Id",
                table: "QuestionQuestionnaire",
                column: "Questionnaire_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeams_TeamId",
                table: "UserTeams",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentHistories");

            migrationBuilder.DropTable(
                name: "DocumentNormalized");

            migrationBuilder.DropTable(
                name: "QuestionQuestionnaire");

            migrationBuilder.DropTable(
                name: "UserTeams");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Questionnaires");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TypeDoc");
        }
    }
}
