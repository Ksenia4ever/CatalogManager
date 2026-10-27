using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncCategoryIndexModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Індекс уже створено попередньою SQL-міграцією.
            // Додаткових змін у базі даних не потрібно.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Відкат не змінює наявний функціональний індекс.
        }
    }
}
