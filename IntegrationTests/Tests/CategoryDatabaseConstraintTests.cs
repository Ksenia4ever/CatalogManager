using CatalogManager.Domain.Entities;
using CatalogManager.Infrastructure.Data;
using CatalogManager.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CatalogManager.IntegrationTests.Tests;

[Collection("DatabaseIntegration")]
public class CategoryDatabaseConstraintTests
{
    [Theory]
    [InlineData("same")]
    [InlineData("lowercase")]
    [InlineData("spaces")]
    public async Task Database_DuplicateCategoryName_IsRejected(
        string variant)
    {
        await using var factory =
            new CatalogManagerWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        var name = "DbCategory" + Guid.NewGuid().ToString("N");

        int originalId = 0;

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var category = new Category
                {
                    Name = name
                };

                db.Categories.Add(category);
                await db.SaveChangesAsync();

                originalId = category.Id;
            }

            var duplicateName = variant switch
            {
                "lowercase" => name.ToLowerInvariant(),
                "spaces" => $"  {name}  ",
                _ => name
            };

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                db.Categories.Add(new Category
                {
                    Name = duplicateName
                });

                // PostgreSQL має відхилити дубльовану назву.
                var exception =
                    await Assert.ThrowsAsync<DbUpdateException>(
                        () => db.SaveChangesAsync());

                var postgresException =
                    Assert.IsType<PostgresException>(
                        exception.InnerException);

                Assert.Equal(
                    PostgresErrorCodes.UniqueViolation,
                    postgresException.SqlState);
            }
        }
        finally
        {
            if (originalId > 0)
            {
                using var scope = factory.Services.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                await db.Categories
                    .Where(c => c.Id == originalId)
                    .ExecuteDeleteAsync();
            }
        }
    }
}