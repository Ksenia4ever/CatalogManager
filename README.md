
# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests and a repeatable Postman API test suite.

The project was developed for the **Web Technologies** coursework:

- **C1 — MVC Application:** CRUD for three related entities with an optional product image URL.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation and HTTP status codes.
- **QA1 — Automated Web API Testing:** repeatable Postman collection with positive and negative scenarios, dynamic IDs, assertions and cleanup.

## Features

- **Categories:** create, read, update and delete; duplicate names are rejected.
- **Stores:** manage name, address, latitude and longitude.
- **Products:** manage name, description, price, optional image URL, category and store.
- **Validation:** required values, price limits, coordinates and valid related entity IDs.
- **Deletion rules:** categories and stores cannot be deleted while referenced by products.
- **MVC interface:** Razor Views, Bootstrap, form ViewModels and antiforgery protection.
- **REST API:** JSON endpoints for Categories, Stores and Products.
- **Testing:** xUnit/Moq, HTTP integration tests and an automated Postman collection.

## Technology Stack

- C# / .NET 10; ASP.NET Core MVC, Web API and Razor Views; Bootstrap
- Entity Framework Core 10; PostgreSQL and Npgsql
- xUnit; Moq; ASP.NET Core `WebApplicationFactory`
- Postman Collection Runner with JavaScript post-response assertions
- Coverlet and ReportGenerator; Git and GitHub

## Solution Structure

```text
CatalogManager/
├── CatalogManager.slnx
├── Frontend/
│   ├── Controllers/           # MVC controllers
│   ├── ApiControllers/        # JSON API controllers
│   ├── Models/                # MVC form ViewModels
│   ├── DTOs/                  # API request/response DTOs
│   ├── Views/                 # Razor Views
│   └── Program.cs
├── Application/               # Services and interfaces
├── Domain/                    # Category, Store and Product entities
├── Infrastructure/            # DbContext, repositories, migrations
├── UnitTests/                 # Unit tests
├── IntegrationTests/          # HTTP + PostgreSQL integration tests
├── Postman/
│   └── CatalogManager_QA1_Automated.postman_collection.json
└── README.md
```

## Architecture and Data Model

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

HTTP client → API Controller → Application Service → Repository → EF Core / PostgreSQL

MVC pages and REST API endpoints use the same business services. Each **Product** belongs to one **Category** and one **Store**; a category or store can have many products. The foreign-key relations restrict deleting referenced categories and stores.

MVC uses `CategoryFormViewModel`, `StoreFormViewModel` and `ProductFormViewModel`. API endpoints use separate create, update and response DTOs to avoid directly exposing database entities.

## MVC Application (C1)

MVC controllers `CategoriesController`, `StoresController` and `ProductsController` provide Create, Read, Update and Delete operations through Razor Views in `Frontend/Views/`.

Input validation uses Data Annotations and service-layer business rules. Form POST actions use antiforgery validation.

## REST API (C5)

| Entity | GET all / POST create | GET one / PUT update / DELETE |
|---|---|---|
| Categories | `/api/categories` | `/api/categories/{id}` |
| Stores | `/api/stores` | `/api/stores/{id}` |
| Products | `/api/products` | `/api/products/{id}` |

Endpoints return:

- `200 OK` — successful reads.
- `201 Created` — successful creation.
- `204 No Content` — successful update or deletion.
- `400 Bad Request` — invalid input.
- `404 Not Found` — missing resources.
- `409 Conflict` — business constraints, such as duplicate category names or attempts to delete referenced categories or stores.

### Example: Create Product

**Request:**

```http
POST /api/products
Content-Type: application/json
```

**JSON body:**

```json
{
  "name": "Lenovo IdeaPad 5",
  "description": "Laptop for everyday use",
  "price": 25000,
  "imageUrl": "https://example.com/laptop.jpg",
  "categoryId": 1,
  "storeId": 1
}
```

The category and store IDs must refer to existing records.

## Automated Postman API Testing (QA1)

The repository contains **one automated Postman collection**:

`Postman/CatalogManager_QA1_Automated.postman_collection.json`

Collection name: **CatalogManager API Tests - Automated QA1**.

The collection contains **50 requests in four folders**, executed in the following order:

1. **Categories (13 requests):** listing, retrieving, creating and updating categories; duplicate-name, invalid JSON and input validation.
2. **Stores (13 requests):** listing, retrieving, creating and updating stores; required fields and coordinate validation.
3. **Products (15 requests):** listing, retrieving, creating and updating products; price, name, foreign-key and JSON validation.
4. **Cleanup and Relationship Checks (9 requests):** deletion conflicts (`409`), deleting dependent resources in the correct order, and verifying subsequent `404` responses.

### How Automation Works

- `Scripts → Pre-request` and `Scripts → Post-response` prepare test data and verify responses using `pm.test` and `pm.expect`.
- Collection variables such as `baseUrl`, `runToken`, `categoryId`, `secondCategoryId`, `storeId` and `productId` connect requests.
- IDs returned by POST requests are stored using `pm.collectionVariables.set(...)` and reused as `{{categoryId}}`, `{{storeId}}`, etc.
- A run-specific token distinguishes newly created test categories from earlier runs.
- Cleanup requests delete products before their stores and categories, respecting foreign-key constraints.
- Negative test cases **pass** when the API returns the expected error status, such as `400`, `404` or `409`.

### Import and Run

1. Start CatalogManager and ensure its HTTPS API is reachable.
2. Import `Postman/CatalogManager_QA1_Automated.postman_collection.json` into Postman.
3. Open the collection's **Variables** tab and check `baseUrl`. The verified local IIS Express value was `https://localhost:44361`, but the port may differ on another machine.
4. Select the **whole collection → Run**.
5. Keep all four folders enabled and in their original order.
6. Select **1 or 2 iterations** and run using Collection Runner.
7. Review **Passed, Failed, Errors** and individual request results.

HTTP statuses `400`, `404` and `409` are expected outcomes for negative tests and do not indicate failed assertions when the corresponding conditions are verified.

The collection requires a running local API and is intended for local or dedicated test environments because its requests create and delete data.

Do not run it against valuable or production data.

### Verified Postman Results

| Run | Iterations | Passed Assertions | Failed | Errors |
|---|---:|---:|---:|---:|
| Initial automated run | 1 | **62** | **0** | **0** |
| Repeatability verification | 2 | **124** | **0** | **0** |

These results were confirmed using Postman Collection Runner on **28 October 2026**.

The numbers represent **Postman assertions**, not xUnit tests.

The successful two-iteration run demonstrates that the automated test workflow can be repeated without manual ID replacement.

## Requirements

- .NET 10 SDK
- Running PostgreSQL server
- Visual Studio with ASP.NET/web development tools or a compatible editor
- Postman for importing and running the automated QA1 collection
- Optional: pgAdmin

## Local Setup

### 1. Clone and Restore

```bash
git clone <YOUR_REPOSITORY_URL>
cd CatalogManager
dotnet restore CatalogManager.slnx
```

### 2. Configure the Development Database

Create the PostgreSQL database `CatalogManagerDb`.

Configure application credentials using **User Secrets**, instead of storing passwords in tracked files:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with your own PostgreSQL password.

User Secrets are intended for local development, not production secret management.

### 3. Apply Migrations and Run

Install the EF Core command-line tool if necessary:

```bash
dotnet tool install --global dotnet-ef
```

Apply migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project Frontend
```

Start the application:

```bash
dotnet run --project Frontend
```

Use the local HTTPS address displayed by the application. The IIS Express port may differ from `44361`.

## C# Automated Testing

The solution contains two testing projects:

| Project | Purpose |
|---|---|
| `UnitTests` | Business services, MVC controllers and API controllers tested using Moq |
| `IntegrationTests` | HTTP workflows through WebApplicationFactory and a separate PostgreSQL test database |

### Test Database Setup

Create a separate database named `CatalogManagerTestDb`.

Configure test credentials through User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

The integration test factory checks the test database name and applies migrations.

### Run All Tests

From the solution root:

```bash
dotnet test CatalogManager.slnx
```

### Last Confirmed C# Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 190 |
| Integration Tests | 27 |
| **Total** | **217** |

**Result: 217 passed, 0 failed, 0 skipped.**

This is the last confirmed Visual Studio Test Explorer result. Additional tests added afterward may change the current count.

These tests are separate from the **62/124 Postman assertions**.

## Code Coverage

Collect code coverage:

```bash
dotnet test CatalogManager.slnx --collect:"XPlat Code Coverage"
```

Install ReportGenerator if necessary:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate the combined HTML report:

```bash
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open `CoverageReport/index.html`.

Exclude stale coverage files when merging results.

The previously reviewed **pre-C5 report** showed:

- **Line coverage:** 76.1%.
- **Branch coverage:** 77.7%.
- **Application layer line coverage:** 100%.

These numbers are not claimed as current C5 or QA1 coverage.

## Security Notes

- Do not commit passwords, tokens, `secrets.json` or other credentials.
- Check exported Postman collections for secrets before publishing.
- MVC POST forms include antiforgery protection.
- API requests are validated using DTOs and application services.
- Do not run data-changing Postman tests against production databases.
- Additional deployment hardening is required before public production use.

## Project Status

- **C1 — MVC Application:** implemented; video defense recorded.
- **C5 — REST API:** implemented, manually tested, and committed to GitHub.
- **QA1 — Automated Web API Testing:** implemented; 62/62 assertions passed in one iteration and 124/124 in two iterations; a separate defense script has been prepared.

Future development may include API pagination, containerization, caching and other coursework stages.
