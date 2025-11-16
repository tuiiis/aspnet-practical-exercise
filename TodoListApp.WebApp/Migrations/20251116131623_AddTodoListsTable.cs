using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoListsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoList_AspNetUsers_OwnerId",
                table: "TodoList");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoTask_TodoList_TodoListId",
                table: "TodoTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoList",
                table: "TodoList");

            migrationBuilder.RenameTable(
                name: "TodoList",
                newName: "TodoLists");

            migrationBuilder.RenameIndex(
                name: "IX_TodoList_OwnerId",
                table: "TodoLists",
                newName: "IX_TodoLists_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoLists",
                table: "TodoLists",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_AspNetUsers_OwnerId",
                table: "TodoLists",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoTask_TodoLists_TodoListId",
                table: "TodoTask",
                column: "TodoListId",
                principalTable: "TodoLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_AspNetUsers_OwnerId",
                table: "TodoLists");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoTask_TodoLists_TodoListId",
                table: "TodoTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoLists",
                table: "TodoLists");

            migrationBuilder.RenameTable(
                name: "TodoLists",
                newName: "TodoList");

            migrationBuilder.RenameIndex(
                name: "IX_TodoLists_OwnerId",
                table: "TodoList",
                newName: "IX_TodoList_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoList",
                table: "TodoList",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoList_AspNetUsers_OwnerId",
                table: "TodoList",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoTask_TodoList_TodoListId",
                table: "TodoTask",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
