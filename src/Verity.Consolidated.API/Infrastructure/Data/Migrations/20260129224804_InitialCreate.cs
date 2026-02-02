using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verity.Consolidated.API.Infrastructure.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyBalances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyBalances_Date",
                table: "DailyBalances",
                column: "Date",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyBalances");
        }
    }
}
