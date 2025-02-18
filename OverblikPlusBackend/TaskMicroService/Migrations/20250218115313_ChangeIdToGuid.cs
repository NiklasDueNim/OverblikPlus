using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMicroService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CalendarEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "CalendarEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CalendarEvents");

            migrationBuilder.AddColumn<int>(
                    name: "Id",
                    table: "CalendarEvents",
                    type: "int",
                    nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents",
                column: "Id");
        }
    }
}