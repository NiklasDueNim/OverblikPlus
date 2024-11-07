using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMicroService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSteps_Tasks_TaskId",
                table: "TaskSteps");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSteps_Tasks_TaskId",
                table: "TaskSteps",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSteps_Tasks_TaskId",
                table: "TaskSteps");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSteps_Tasks_TaskId",
                table: "TaskSteps",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
