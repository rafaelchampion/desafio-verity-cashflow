using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verity.Consolidated.API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnableOptimisticConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // xmin is a system column in PostgreSQL and always exists.
            // We just need EF Core to be aware of it (via ModelSnapshot),
            // so no SQL operation is required here.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to revert as xmin is a system column.
        }
    }
}
