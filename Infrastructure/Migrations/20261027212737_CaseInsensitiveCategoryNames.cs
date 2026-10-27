using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseInsensitiveCategoryNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        DROP INDEX IF EXISTS "IX_Categories_Name";

        CREATE UNIQUE INDEX "IX_Categories_Name"
        ON "Categories" (LOWER(TRIM("Name")));
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        DROP INDEX IF EXISTS "IX_Categories_Name";

        CREATE UNIQUE INDEX "IX_Categories_Name"
        ON "Categories" ("Name");
        """);
        }
    }
}
