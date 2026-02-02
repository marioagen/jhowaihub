using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WoopiAiHub.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_Label_Column_On_Status_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Status",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE [Status] 
                SET [Label] = 'workflow.statusList.awaitinganalysis'
                WHERE [Name] = 'AwaitingAnalysis';
            ");

            migrationBuilder.Sql(@"
                UPDATE [Status] 
                SET [Label] = 'workflow.statusList.analyzed'
                WHERE [Name] = 'Analyzed';
            ");

            migrationBuilder.Sql(@"
                UPDATE [Status] 
                SET [Label] = 'workflow.statusList.waitingforapproval'
                WHERE [Name] = 'WaitingForApproval';
            ");

            migrationBuilder.Sql(@"
                UPDATE [Status] 
                SET [Label] = 'workflow.statusList.approved'
                WHERE [Name] = 'Approved';
            ");

            migrationBuilder.Sql(@"
                UPDATE [Status] 
                SET [Label] = 'workflow.statusList.done'
                WHERE [Name] = 'Done';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Status");
        }
    }
}
