using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _3ASPC_Proj.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_UserTaskTaskId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserTaskTaskId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UserTaskTaskId",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionRaw",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionRaw",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "UserTaskTaskId",
                table: "Tasks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserTaskTaskId",
                table: "Tasks",
                column: "UserTaskTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_UserTaskTaskId",
                table: "Tasks",
                column: "UserTaskTaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId");
        }
    }
}
