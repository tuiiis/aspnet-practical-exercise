using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserIdToTodoTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "TodoTasks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_AssignedUserId",
                table: "TodoTasks",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoTasks_AspNetUsers_AssignedUserId",
                table: "TodoTasks",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoTasks_AspNetUsers_AssignedUserId",
                table: "TodoTasks");

            migrationBuilder.DropIndex(
                name: "IX_TodoTasks_AssignedUserId",
                table: "TodoTasks");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "TodoTasks");
        }
    }
}
