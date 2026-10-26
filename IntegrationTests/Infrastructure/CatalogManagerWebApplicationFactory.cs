using CatalogManager.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace CatalogManager.IntegrationTests.Infrastructure;

public class CatalogManagerWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private string? _testConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Вказуємо актуальну папку MVC-проєкту.
        // Раніше вона називалася CatalogManager,
        // тепер вона називається Frontend.
        var solutionDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", ".."));

        var frontendDirectory = Path.Combine(
            solutionDirectory, "Frontend");

        if (!Directory.Exists(frontendDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Frontend project directory not found: {frontendDirectory}");
        }

        builder.UseContentRoot(frontendDirectory);

        builder.UseEnvironment("Testing");

        // Завантажуємо рядок підключення з User Secrets.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<CatalogManagerWebApplicationFactory>()
            .Build();

        _testConnectionString = configuration
            .GetConnectionString("TestConnection");

        if (string.IsNullOrWhiteSpace(_testConnectionString))
        {
            throw new InvalidOperationException(
                "TestConnection is not configured in User Secrets.");
        }

        // Перевіряємо, що тести використовують окрему БД.
        var connectionBuilder =
            new NpgsqlConnectionStringBuilder(
                _testConnectionString);

        if (!string.Equals(
            connectionBuilder.Database,
            "CatalogManagerTestDb",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests must use CatalogManagerTestDb.");
        }

        // Замінюємо основне підключення до PostgreSQL
        // підключенням до тестової бази даних.
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(
                    _testConnectionString));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();
    }
}