using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "Name", "Created" },
                values: new object[,]
                {
                    { 1, "Admin", DateTime.Now }
                }
            );

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "Name", "Created" },
                values: new object[,]
                {
                   { 1, "IA", DateTime.Now },
                   { 2, "Admin", DateTime.Now }
                }
            );

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Created", "Description", "Group" },
                values: new object[,]
                {
                    { 1, "View",   DateTime.Now, "View Question",        "Questions" },
                    { 2, "View",   DateTime.Now, "View Type",            "Types" },
                    { 3, "View",   DateTime.Now, "View Quizz",           "Quizzes" },
                    { 4, "View",   DateTime.Now, "View Documents",       "Documents" },
                    { 5, "View",   DateTime.Now, "View Management Tables","Management" },
                    { 6, "View",   DateTime.Now, "View Users",           "Users" },
                    { 7, "View",   DateTime.Now, "View Teams",           "Teams" },
                    { 8, "View",   DateTime.Now, "View Profiles",        "Profiles" },
                    { 9, "View",   DateTime.Now, "View Workflow",        "Workflow" },
                    { 10, "View",   DateTime.Now, "View Tools",           "Tools" },
                    { 11, "View",   DateTime.Now, "View Step",            "Workflow-Step" },
                    { 12, "Access", DateTime.Now, "Access Step",          "Workflow-Step" },
                    { 13, "View",   DateTime.Now, "View Dashboard",       "Dashboard" },
                }
            );


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
