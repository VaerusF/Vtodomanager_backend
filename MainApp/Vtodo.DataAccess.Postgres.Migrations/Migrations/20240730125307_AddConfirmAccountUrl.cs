using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vtodo.DataAccess.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmAccountUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfirmAccountUrls",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    UrlPart = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmAccountUrls", x => new { x.AccountId, x.UrlPart });
                    table.ForeignKey(
                        name: "FK_ConfirmAccountUrls_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmAccountUrls");
        }
    }
}
