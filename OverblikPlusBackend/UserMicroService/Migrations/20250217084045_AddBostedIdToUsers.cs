using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMicroService.Migrations
{
    /// <inheritdoc />
    public partial class AddBostedIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BostedId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BostedId",
                table: "AspNetUsers");
        }
    }
}