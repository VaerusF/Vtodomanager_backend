using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VtodoManager.NewsService.DataAccess.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_News_Title_Content",
                table: "News",
                columns: new[] { "Title", "Content" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_News_Title_Content",
                table: "News");
        }
    }
}
