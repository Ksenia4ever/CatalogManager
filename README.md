# CatalogManager

CatalogManager is a layered ASP.NET Core MVC application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated unit and integration tests.

## Features

- **Categories:** create, view, edit, and delete categories.
- **Stores:** manage store names, addresses, and geographic coordinates.
- **Products:** manage product names, descriptions, prices, and optional image URLs; assign each product to a category and a store.
- **Validation:** reject invalid product prices, empty required fields, invalid coordinates, and references to nonexistent categories or stores.
- **Deletion rules:** prevent deleting a category or store while products reference it.
- **MVC interface:** Razor views with Bootstrap styling and forms protected by antiforgery validation.

## Technology stack

- C# / .NET 10
- ASP.NET Core MVC and Razor Views
- Entity Framework Core 10
- PostgreSQL with Npgsql
- xUnit and Moq (unit testing)
- ASP.NET Core `WebApplicationFactory` (HTTP integration testing)
- Coverlet and ReportGenerator (code coverage)

## Solution structure

```text
CatalogManager/
├── CatalogManager.slnx
├── Frontend/          # ASP.NET Core MVC: controllers, view models, Razor views
├── Application/       # Business services and repository/service interfaces
├── Domain/            # Category, Store, and Product entities
├── Infrastructure/    # EF Core DbContext, repositories, and migrations
├── UnitTests/         # Service and controller unit tests
└── IntegrationTests/  # HTTP tests running against a separate PostgreSQL database
```

### Architecture

Requests flow through the **MVC controllers → application services → repositories → EF Core / PostgreSQL**. Controllers use service interfaces, while services use repository interfaces. The infrastructure layer implements those repositories. This separation makes business rules easier to test without a live database.

### Data model

- One **Category** can contain many **Products**.
- One **Store** can contain many **Products**.
- Each **Product** belongs to one Category and one Store.

The foreign-key relationships restrict deletion of a Category or Store that still has Products.

## Requirements

- .NET 10 SDK
- PostgreSQL server (local development setup)
- Visual Studio with ASP.NET and web development tooling, or a compatible editor/CLI
- Optional: pgAdmin for database inspection

## Local setup

### 1. Clone and restore

```bash
git clone <YOUR_REPOSITORY_URL>
cd CatalogManager
dotnet restore CatalogManager.slnx
```

### 2. Create the application database

Create a PostgreSQL database named `CatalogManagerDb` on your local PostgreSQL server (the default configuration expects `localhost:5432` and user `postgres`).

The committed `Frontend/appsettings.json` intentionally contains **no password**. Keep credentials out of tracked configuration files.

From the solution root, configure **User Secrets** for the MVC project:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Use your own PostgreSQL credentials. User Secrets are intended for local development, not as a production secret store.

### 3. Apply EF Core migrations

Install the EF Core CLI tool if needed (`dotnet tool install --global dotnet-ef`) and run:

```bash
dotnet ef database update --project Infrastructure --startup-project Frontend
```

This creates or updates the `Categories`, `Stores`, and `Products` tables and the EF migrations history table.

### 4. Start the application

```bash
dotnet run --project Frontend
```

Open the local URL shown in the terminal. Navigate to **Categories**, **Stores**, and **Products** to manage the catalog.

> Create at least one category and one store before adding a product, since products require both relationships.

## Testing

The solution contains two test projects:

| Project | Purpose |
| --- | --- |
| `UnitTests` | Service business rules and MVC controller actions, with mocked dependencies |
| `IntegrationTests` | HTTP requests through the MVC application, form submission, antiforgery handling, CRUD operations, and PostgreSQL persistence |

### Test database setup

Create a separate PostgreSQL database named **`CatalogManagerTestDb`**. The integration test factory explicitly checks this database name; do not point tests at the development database.

Configure User Secrets for the integration test project:

```bash
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

The integration test factory applies migrations to the test database. Tests should be run against this dedicated database, not one containing valuable data.

Run all tests from the solution root:

```bash
dotnet test CatalogManager.slnx
```

**Most recently confirmed run:** 164 passing tests (150 unit tests and 14 integration test cases), with zero failures and zero skipped tests.

### Code coverage

Collect coverage with Coverlet:

```bash
dotnet test CatalogManager.slnx --collect:"XPlat Code Coverage"
```

To produce a merged HTML report, install ReportGenerator and point it to the current Coverlet reports:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open `CoverageReport/index.html`. Remove or exclude stale test runs before merging, otherwise old reports may affect the resulting statistics.

**Last reviewed combined report:** 76.1% line coverage and 77.7% branch coverage. Application business logic reached 100% line coverage. Coverage describes executed code, not a guarantee that the application is free of defects.

## Security notes

- Never commit PostgreSQL passwords, `secrets.json`, API keys, or other secrets.
- MVC POST actions use antiforgery validation.
- The product create/edit forms use a dedicated `ProductFormViewModel`.
- This is an educational project; review configuration, validation, and deployment hardening before exposing it publicly.

## Project status

The MVC catalog and automated test suite are implemented. Additional improvements can include stronger form-model separation for categories and stores, richer error handling, and a map integration that uses store coordinates.
