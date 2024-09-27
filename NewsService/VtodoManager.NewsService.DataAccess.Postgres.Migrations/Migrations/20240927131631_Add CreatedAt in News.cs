using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VtodoManager.NewsService.DataAccess.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtinNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "News",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "News");
        }
    }
}
