
# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests, Postman API testing, and Playwright browser-based E2E testing.

The project was developed for the **Web Technologies** coursework.

## Completed Coursework Stages

- **C1 — MVC Application:** CRUD operations for three related entities with an optional product image URL.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation, and HTTP status codes.
- **QA1 — Automated Web API Testing:** Postman collection with positive and negative tests, assertions, dynamic IDs, and cleanup.
- **B1 — Pagination API:** offset-based pagination using `skip`, `limit`, and `nextLink`.
- **C8 — External API Integration:** interactive stores map powered by OpenStreetMap and Leaflet.
- **B9 — API Output Caching:** in-memory response caching with expiration and tag-based invalidation.
- **C4 — Docker Containerization:** ASP.NET Core and PostgreSQL containers using Docker Compose.
- **C6 — Bootstrap Landing Page:** responsive product catalog and individual product landing pages.
- **C7 — SEO Optimization:** product-specific metadata, canonical URLs, Open Graph tags, robots.txt, and Lighthouse auditing.
- **F1 — Data Visualization:** statistics dashboard using Chart.js and PostgreSQL data.
- **QA2 — End-to-End Testing:** automated Chromium browser tests using Microsoft Playwright and xUnit.

---

## Features

- **Categories:** create, read, update, and delete categories; reject duplicate names.
- **Stores:** manage store names, addresses, and geographic coordinates.
- **Products:** manage names, descriptions, prices, images, categories, and stores.
- **Validation:** required fields, prices, geographic coordinates, and related entity IDs.
- **Deletion constraints:** prevent deleting referenced categories and stores.
- **MVC interface:** Razor Views, Bootstrap, ViewModels, and antiforgery validation.
- **REST API:** CRUD endpoints for categories, stores, and products.
- **Pagination:** offset-based pagination for collection endpoints.
- **Interactive map:** store markers, popups, search, and refresh.
- **Output caching:** 30-second in-memory cache with tag-based invalidation.
- **Docker:** ASP.NET Core and PostgreSQL with persistent database storage.
- **Product landing pages:** responsive cards, images, descriptions, and prices.
- **SEO:** product-specific metadata, canonical URLs, Open Graph, and robots.txt.
- **Statistics:** three Chart.js charts and asynchronously refreshed data.
- **Automated testing:** xUnit, Moq, HTTP integration tests, Postman, and Playwright E2E tests.

---

## Technology Stack

- C# / .NET 10
- ASP.NET Core MVC, Web API, and Razor Views
- Bootstrap, HTML, CSS, and JavaScript
- Chart.js
- Entity Framework Core 10
- PostgreSQL 17 and Npgsql
- ASP.NET Core Output Caching
- Docker and Docker Compose
- xUnit and Moq
- ASP.NET Core WebApplicationFactory
- Microsoft Playwright for .NET
- Chromium browser automation
- Postman Collection Runner
- OpenStreetMap and Leaflet.js
- Google Chrome Lighthouse
- Coverlet and ReportGenerator
- Git and GitHub

---

## Solution Structure

```text
CatalogManager/
├── CatalogManager.slnx
├── Frontend/
│   ├── Controllers/
│   │   └── StatisticsController.cs
│   ├── ApiControllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Views/
│   │   ├── Products/
│   │   │   ├── Index.cshtml
│   │   │   └── Details.cshtml
│   │   ├── StoresMap/
│   │   ├── Statistics/
│   │   │   └── Index.cshtml
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   ├── wwwroot/
│   │   ├── js/
│   │   │   ├── stores-map.js
│   │   │   └── statistics.js
│   │   ├── css/
│   │   │   ├── stores-map.css
│   │   │   ├── product-landing.css
│   │   │   └── statistics.css
│   │   └── robots.txt
│   └── Program.cs
├── Application/
├── Domain/
├── Infrastructure/
├── UnitTests/
├── IntegrationTests/
│   └── Tests/
│       └── StatisticsIntegrationTests.cs
├── E2ETests/
│   ├── CatalogManager.E2ETests.csproj
│   └── CatalogManagerBrowserTests.cs
├── Postman/
│   └── CatalogManager_QA1_B1_B9.postman_collection.json
├── Dockerfile
├── compose.yaml
├── .dockerignore
├── .env.example
├── .gitignore
├── B1_PAGINATION_GUIDE.md
└── README.md
```

Local `.env` files contain environment-specific Docker configuration and must not be committed to Git.

---

## Architecture and Data Model

CatalogManager follows a layered architecture.

**MVC requests:**

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

**REST API requests:**

HTTP Client → API Controller → Application Service → Repository → EF Core / PostgreSQL

The MVC application and REST API share application services and business rules.

Each Product belongs to one Category and one Store. A Category or Store may contain multiple Products.

Foreign-key constraints prevent deleting categories or stores referenced by products.

MVC uses dedicated form ViewModels:

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

REST API endpoints use separate request and response DTOs.

The Statistics dashboard reads existing catalog records and prepares aggregated JSON data for visualization.

---

## C1 — MVC Application

MVC controllers `CategoriesController`, `StoresController`, and `ProductsController` provide Create, Read, Update, and Delete operations through Razor Views.

Input validation uses Data Annotations and application service business rules.

Form POST actions use antiforgery validation.

The MVC interface also provides the foundation for the C6 product landing pages and the browser-based QA2 tests.

---

## C5 — REST API

| Entity | Collection Endpoint | Resource Endpoint |
|---|---|---|
| Categories | `/api/categories` | `/api/categories/{id}` |
| Stores | `/api/stores` | `/api/stores/{id}` |
| Products | `/api/products` | `/api/products/{id}` |

Supported HTTP methods:

- GET
- POST
- PUT
- DELETE

### HTTP Status Codes

- `200 OK` — successful reads.
- `201 Created` — successful creation.
- `204 No Content` — successful update or deletion.
- `400 Bad Request` — invalid input.
- `404 Not Found` — missing resource.
- `409 Conflict` — business rule violations.

### Example: Create Product

```http
POST /api/products
Content-Type: application/json
```

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

Category and store IDs must refer to existing records.

---

## QA1 — Automated Postman API Testing

The original QA1 collection contains 50 requests in four folders:

1. **Categories (13):** CRUD, duplicate names, invalid JSON, and validation.
2. **Stores (13):** CRUD, required fields, and coordinate validation.
3. **Products (15):** CRUD, price validation, foreign keys, and invalid JSON.
4. **Cleanup and Relationship Checks (9):** deletion conflicts and cleanup.

### Automation

- JavaScript pre-request and post-response scripts.
- Assertions with `pm.test()` and `pm.expect()`.
- Dynamic IDs stored in collection variables.
- Run-specific tokens for test data.
- Cleanup respecting foreign-key relationships.
- Positive and negative test scenarios.

### Verified QA1 Results

| Iterations | Passed Assertions | Failed | Errors |
|---|---:|---:|---:|
| 1 | 62 | 0 | 0 |
| 2 | 124 | 0 | 0 |

Verified on 28 October 2026.

---

## B1 — Pagination API

B1 implements offset-based pagination for REST API collection endpoints.

### Example Requests

```http
GET /api/categories?skip=0&limit=10
GET /api/stores?skip=0&limit=10
GET /api/products?skip=0&limit=10
```

### Parameters

- `skip` — number of records to skip.
- `limit` — maximum number of records returned.
- `nextLink` — URL of the next page, if available.

Paginated responses contain:

- `items`
- `skip`
- `limit`
- `totalCount`
- `nextLink`

Requests without pagination parameters preserve the original C5 JSON array response.

Pagination is performed at the database query level using Entity Framework Core.

See [B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md) for further documentation.

---

## C8 — Interactive Stores Map

C8 integrates OpenStreetMap and Leaflet.js.

The page is available at:

`/StoresMap`

Store information is retrieved through:

```http
GET /api/stores
```

### Features

- OpenStreetMap tile rendering.
- Store markers based on saved coordinates.
- Popups with store information.
- Search by store name.
- Visible marker counter.
- Refresh without full page reload.
- Coordinate validation.
- Navigation through the application menu.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | MVC controller |
| `Frontend/Views/StoresMap/Index.cshtml` | Map View |
| `Frontend/wwwroot/js/stores-map.js` | Map logic |
| `Frontend/wwwroot/css/stores-map.css` | Map styling |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests |

Three C8 integration test cases verify the map page and static resources.

---

## B9 — API Output Caching

B9 implements server-side response caching using ASP.NET Core Output Cache.

### Cache Policies

| Policy | API | Duration | Tag |
|---|---|---|---|
| `CategoriesCache` | Categories | 30 seconds | `categories` |
| `StoresCache` | Stores | 30 seconds | `stores` |
| `ProductsCache` | Products | 30 seconds | `products` |

The application uses `AddOutputCache()` and `UseOutputCache()`.

Eligible GET responses may be served from memory.

Successful API POST, PUT, and DELETE operations invalidate relevant cached data using `IOutputCacheStore.EvictByTagAsync()`.

Direct database and MVC changes are not automatically covered by API-controller invalidation.

### Postman Verification

The `B9 - API Caching` folder contains 24 requests.

| Metric | Result |
|---|---:|
| Requests | 24 |
| Assertions | 48 |
| Passed | 48 |
| Failed | 0 |
| Errors | 0 |

Verified on 29 October 2026.

These assertions verify HTTP behavior and data visibility. Direct Cache HIT verification would require server-side diagnostics.

### Limitations

- Memory cache is local to one application instance.
- Cache entries expire after 30 seconds.
- Cache data is lost on application restart.
- The cache is not shared across multiple containers.
- Direct database or MVC updates may remain invisible until expiration.

---

## C4 — Docker Containerization

C4 runs ASP.NET Core and PostgreSQL in separate Linux containers managed by Docker Compose.

### Architecture

```text
Windows Host
    |
    | http://localhost:8085
    v
Docker Compose
    |
    +--- web
    |    ASP.NET Core 10
    |    Port 8080
    |       |
    |       | db:5432
    |       v
    +--- db
         PostgreSQL 17
         Port 5432
            |
            v
         Docker Volume
         catalogmanager_pgdata
```

The application is accessible at:

`http://localhost:8085`

The database is exposed locally at:

`127.0.0.1:55432`

### Docker Files

| File | Responsibility |
|---|---|
| `Dockerfile` | Multi-stage .NET build |
| `.dockerignore` | Build context exclusions |
| `compose.yaml` | Container services |
| `.env.example` | Example configuration |
| `.env` | Local settings, excluded from Git |

### Start Docker Environment

From Windows PowerShell in the solution root:

```powershell
docker compose up --build -d
```

Check container status:

```powershell
docker compose ps -a
```

Expected state:

- PostgreSQL container: Healthy.
- ASP.NET Core container: Up.

### Database Migrations

For a new Docker database, apply existing EF Core migrations.

Set a temporary connection string in Windows PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=55432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_DOCKER_PASSWORD"
```

Apply migrations:

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

Remove the temporary override:

```powershell
Remove-Item Env:ConnectionStrings__DefaultConnection
```

### Useful Docker Commands

Restart containers:

```powershell
docker compose restart
```

Stop containers without removing the database volume:

```powershell
docker compose down
```

Start existing containers:

```powershell
docker compose up -d
```

View application logs:

```powershell
docker compose logs web --tail=80
```

Do not use `docker compose down -v` unless the database volume should intentionally be deleted.

### Verified Results

- Docker Compose configuration passed validation.
- ASP.NET Core and PostgreSQL containers started.
- Database migrations were applied.
- MVC and REST API requests worked.
- Data remained stored after restarting containers.
- The Docker environment was reused successfully during QA2.
- All seven Playwright E2E tests passed against the Docker application.

### Limitations

The configuration is intended for local development and coursework demonstration, not production deployment.

---

## C6 — Bootstrap Product Landing Pages

C6 improves the presentation of products through responsive Bootstrap pages.

### Catalog

`/Products`

The product catalog displays responsive product cards.

### Individual Product Page

`/Products/Details/{id}`

Example:

`/Products/Details/1`

### Features

- Responsive Bootstrap cards.
- Dedicated product landing pages.
- Product name, description, and price.
- Optional product images.
- Category and store information.
- Navigation between catalog and details pages.
- Responsive styling through custom CSS.

### Files

| File | Responsibility |
|---|---|
| `Frontend/Views/Products/Index.cshtml` | Product catalog |
| `Frontend/Views/Products/Details.cshtml` | Landing page |
| `Frontend/wwwroot/css/product-landing.css` | Custom CSS |

The existing MVC controller, services, and database schema are reused.

No new migrations were needed.

---

## C7 — SEO Optimization

C7 improves technical SEO for the product landing pages introduced in C6.

### Features

- Descriptive, product-specific page titles.
- Product-based meta descriptions.
- Canonical URLs.
- Open Graph metadata.
- Search crawler instructions in `robots.txt`.
- Lighthouse SEO auditing.

### Robots.txt

File:

`Frontend/wwwroot/robots.txt`

URL:

`/robots.txt`

Contents:

```text
User-agent: *
Allow: /
```

The file permits crawling according to the stated rules but does not guarantee indexing.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Views/Shared/_Layout.cshtml` | Shared SEO metadata |
| `Frontend/Views/Products/Details.cshtml` | Product metadata |
| `Frontend/wwwroot/robots.txt` | Crawler instructions |

### Lighthouse Audit

Audited URL:

`https://localhost:44361/Products/Details/1`

Configuration:

- Navigation mode.
- Desktop device.
- SEO category.

| Metric | Result |
|---|---:|
| SEO Score | **100/100** |
| Passed Audits | 9 |
| Not Applicable | 1 |
| Failed Applicable Audits | 0 |

Verified on 30 October 2026.

The score applies to the audited local page and does not guarantee search engine ranking.

---

## F1 — Data Visualization

F1 adds an interactive statistical dashboard using Chart.js.

### Statistics Page

`/Statistics`

### Data Endpoint

```http
GET /Statistics/Data
```

The endpoint returns JSON generated from existing PostgreSQL records.

### Dashboard Indicators

- Total product count.
- Total category count.
- Total store count.

### Charts

1. **Products by Category:** number of products associated with each category.
2. **Products by Store:** number of products associated with each store.
3. **Price Distribution:** products grouped into price ranges.

### Interactive Features

- Automatic data loading.
- Fetch API requests.
- Chart.js rendering.
- Refresh button.
- Loading and error handling.
- Responsive layout.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StatisticsController.cs` | Page and JSON endpoint |
| `Frontend/Views/Statistics/Index.cshtml` | Dashboard View |
| `Frontend/wwwroot/js/statistics.js` | Fetch and chart rendering |
| `Frontend/wwwroot/css/statistics.css` | Styling |
| `IntegrationTests/Tests/StatisticsIntegrationTests.cs` | Integration tests |

Four additional integration test cases were introduced.

### Verified F1 Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 59 |
| **Total** | **267** |

Verified on 30 October 2026.

---

## QA2 — End-to-End Testing with Playwright

### Overview

QA2 introduces browser-based End-to-End testing using **Microsoft Playwright for .NET**, **xUnit**, and **Chromium**.

Unlike unit and HTTP integration tests, E2E tests interact with the application as a real user would.

They verify navigation, form interaction, validation, and the availability of important user interface pages.

The tests run against a live CatalogManager instance.

### QA2 Test Project

Project:

`E2ETests/CatalogManager.E2ETests.csproj`

Main test class:

`E2ETests/CatalogManagerBrowserTests.cs`

The project targets .NET 10 and uses:

- `Microsoft.Playwright`
- `xunit`
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`
- `coverlet.collector`

### Implemented E2E Scenarios

| Test | Scenario | Result |
|---|---|---|
| QA2-01 | Home page and navigation to Products | Passed |
| QA2-02 | Category Create → Edit → Delete | Passed |
| QA2-03 | Required category name validation | Passed |
| QA2-04 | Product catalog and Create form | Passed |
| QA2-05 | Stores Map search and refresh controls | Passed |
| QA2-06 | Statistics dashboard and refresh control | Passed |
| QA2-07 | SEO robots.txt availability | Passed |

### Test Execution

The tests use Playwright to control a Chromium browser.

The browser runs in headless mode.

The test project uses an environment variable to configure the tested application URL:

`CATALOGMANAGER_E2E_BASE_URL`

The default URL is:

`https://localhost:44361`

The Docker URL is:

`http://localhost:8085`

### Install Playwright Chromium

Build the test project:

```powershell
dotnet build E2ETests/CatalogManager.E2ETests.csproj
```

Install Chromium using Windows PowerShell:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\E2ETests\bin\Debug\net10.0\playwright.ps1" install chromium
```

This installs the browser components required for Playwright testing.

### Run QA2 with Visual Studio and IIS Express (Without Docker)

1. Configure the local PostgreSQL database and application connection string.
2. Open `CatalogManager.slnx` in Visual Studio.
3. Select the IIS Express launch profile.
4. Start the application using **Ctrl + F5** (Start Without Debugging).
5. Verify the following page opens:

`https://localhost:44361/Products/Create`

6. Open **Developer PowerShell** in the solution root.
7. Set the application URL:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"
```

8. Run the E2E tests:

```powershell
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

Expected result:

```text
Test summary: total: 7; failed: 0; succeeded: 7; skipped: 0
```

Keep IIS Express running throughout the test execution.

The test project uses the existing ASP.NET Core application and does not require Docker when IIS Express is available.

### Run QA2 Tests Against Docker

Docker provides an alternative test environment.

Start the Docker environment:

```powershell
docker compose up --build -d
```

Verify that the application is accessible at:

`http://localhost:8085`

Set the E2E base URL in the current Windows PowerShell session:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "http://localhost:8085"
```

Run the E2E tests:

```powershell
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Verified QA2 Results

The Playwright E2E test suite was successfully executed against both IIS Express and Docker.

| Environment | Total | Passed | Failed | Skipped | Duration |
|---|---:|---:|---:|---:|---|
| Visual Studio + IIS Express | 7 | 7 | 0 | 0 | 9.7 seconds |
| Docker + PostgreSQL | 7 | 7 | 0 | 0 | 9.2 seconds |

**Verification date:** 30 October 2026.

**Result: All seven E2E tests passed in both environments.**

These results demonstrate that QA2 does not depend specifically on Docker and can be executed against an independently running ASP.NET Core application.

### E2E Scenario Coverage Assessment

QA2 verifies seven selected end-user scenarios across the main application interfaces.

| Functional Area | Covered Scenario | Coverage Assessment |
|---|---|---|
| Navigation | Home → Products | Covered |
| Categories | Complete Create, Edit, Delete flow | Covered |
| Categories Validation | Required Name | Covered |
| Products | Catalog and Create form accessibility | Partially covered |
| Stores Map | Page and search/refresh controls | Partially covered |
| Statistics | Page and refresh control | Partially covered |
| SEO | robots.txt availability | Covered |
| Stores CRUD | Full Create, Edit, Delete flow | Not covered by QA2 |
| Products CRUD | Full Create, Edit, Delete flow | Not covered by QA2 |
| REST API Negative Scenarios | HTTP validation and conflict responses | Covered separately by QA1 |

### Coverage Interpretation

The E2E suite covers selected core user journeys but does not represent complete coverage of all application features.

The main full CRUD browser scenario covers Categories.

Products, Stores Map, and Statistics are covered by selected user interface scenarios, while their complete behavior is not exhaustively verified through QA2.

This assessment represents functional scenario coverage, not source-code line coverage.

### Test Data and Isolation

The Category CRUD E2E scenario creates a uniquely named test category.

After successful execution, it deletes the test record.

This supports repeatable execution and reduces interference with existing catalog records.

The test suite runs against a live development environment and should not be executed against production data.

### Docker and IIS Express Compatibility

During the initial QA2 implementation, Windows Application Control temporarily prevented IIS Express from loading `CatalogManager.Domain.dll`.

Docker provided a working alternative environment for testing.

After rebuilding the Docker image, all seven E2E tests passed against:

`http://localhost:8085`

The local IIS Express issue was subsequently resolved.

The same seven tests then passed against:

`https://localhost:44361`

No changes to the E2E test source code were required to switch between environments.

### QA2 Limitations

- Tests require a running CatalogManager instance.
- Playwright Chromium must be installed.
- Tests currently use headless Chromium.
- Only selected user journeys are covered.
- Full browser CRUD testing for all three entities is not included.
- Running browser tests against production data is not recommended.
- E2E tests do not replace unit or HTTP integration tests.
- A clean development database and valid application configuration are required.

### QA2 Conclusion

QA2 extends CatalogManager with automated real-browser testing.

The project now supports three complementary testing levels:

1. Unit tests for isolated application behavior.
2. HTTP integration tests for application endpoints.
3. Browser-based E2E tests for user workflows.

**QA2 result: 7 passed, 0 failed, 0 skipped in both verified environments.**

---

## Postman Collection — QA1 + B1 + B9

The combined Postman collection is stored at:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

It contains scenarios for:

- QA1 automated API testing.
- B1 pagination.
- B9 output caching.

### Run the Collection

1. Start CatalogManager.
2. Import the collection into Postman.
3. Set the `baseUrl` collection variable.
4. Select the relevant folder.
5. Run the folder using Collection Runner.
6. Review Passed, Failed, and Errors.

Local IIS Express URL:

`https://localhost:44361`

Docker URL:

`http://localhost:8085`

Run data-changing tests only against development or test environments.

---

## Local Setup Without Docker

### Restore Dependencies

```powershell
dotnet restore CatalogManager.slnx
```

### Database Configuration

Create the PostgreSQL database:

`CatalogManagerDb`

Configure the connection using User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with the local PostgreSQL password.

### Apply Migrations

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

### Run Application

```powershell
dotnet run --project Frontend
```

The development URL depends on the selected launch profile.

Alternatively, open the solution in Visual Studio and run CatalogManager using IIS Express.

---

## Automated Testing

The solution contains three testing projects:

| Project | Purpose |
|---|---|
| `UnitTests` | Unit testing using xUnit and Moq |
| `IntegrationTests` | HTTP integration testing with WebApplicationFactory and PostgreSQL |
| `E2ETests` | Browser-based testing using Playwright and Chromium |

### Integration Test Database

The HTTP integration tests use a separate PostgreSQL database:

`CatalogManagerTestDb`

Configure it using User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

### Run Unit and Integration Tests

```powershell
dotnet test UnitTests
dotnet test IntegrationTests
```

### Run E2E Tests with IIS Express

Make sure CatalogManager is running at:

`https://localhost:44361`

In Developer PowerShell:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Run E2E Tests with Docker

Make sure CatalogManager is running at:

`http://localhost:8085`

In Windows PowerShell:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "http://localhost:8085"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

Both environments were verified successfully.

### Last Confirmed Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 59 |
| Playwright E2E Tests | 7 |
| **Total** | **274** |

**274 tests passed across the three test suites, with 0 failed and 0 skipped in their confirmed runs.**

The 267 unit/integration tests and seven Playwright E2E tests were verified separately.

The E2E tests passed against both IIS Express and Docker on 30 October 2026.

---

## Code Coverage

Collect C# code coverage:

```powershell
dotnet test CatalogManager.slnx --collect:"XPlat Code Coverage"
```

Install ReportGenerator if needed:

```powershell
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate a combined coverage report:

```powershell
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open:

`CoverageReport/index.html`

A previously reviewed pre-C5 coverage report showed:

- Line coverage: 76.1%.
- Branch coverage: 77.7%.
- Application layer line coverage: 100%.

These are historical measurements and are not claimed as current QA2 code coverage.

The QA2 scenario assessment is separate from code coverage.

---

## Security Notes

- Never commit PostgreSQL passwords, tokens, or secret connection strings.
- Keep `.env` files excluded from Git.
- Use `.env.example` to document configuration.
- Use User Secrets for local development.
- Check exported Postman collections for secrets.
- Run data-changing tests only against test or development databases.
- MVC forms use antiforgery protection.
- REST API inputs are validated by DTOs and business rules.
- Docker PostgreSQL is exposed only on localhost.
- `robots.txt` is not an access-control mechanism.
- Do not disable Windows security policies to bypass blocked DLLs.
- Production deployment requires additional security hardening.

---

## Project Status

### Implemented and Verified

- **C1 — MVC Application:** implemented; video defense recorded.
- **C5 — REST API:** implemented and tested.
- **QA1 — Automated Web API Testing:** 62/62 and 124/124 assertions passed.
- **B1 — Pagination API:** implemented and verified.
- **C8 — Interactive Stores Map:** implemented and verified.
- **B9 — API Output Caching:** implemented; 48/48 Postman assertions passed.
- **C4 — Docker Containerization:** built and verified with PostgreSQL 17 and persistent storage.
- **C6 — Bootstrap Landing Page:** implemented and verified.
- **C7 — SEO Optimization:** Lighthouse SEO 100/100.
- **F1 — Data Visualization:** three charts and statistics dashboard implemented; 267 C# tests passed.
- **QA2 — End-to-End Testing:** seven Playwright browser tests passed against both IIS Express and Docker.

### Latest Testing Summary

- 208 unit tests passed.
- 59 HTTP integration tests passed.
- 7 Playwright E2E tests passed.
- **274 total tests passed across the confirmed test runs.**

### Remaining Tasks

- Verify that local `.env` files are not tracked by Git.
- Independently confirm actual B9 Cache HIT using diagnostics.
- Implement and verify F2 and B2.
- Prepare a combined F1, QA2, F2, and B2 video defense after completing the next two stages.

### Planned Next Stages

- **F2 — AJAX Autocomplete (4 points):** replace Category and Store dropdowns in Product forms with server-side autocomplete.
- **B2 — External API Integration (2 points):** add integration with another external API for currency information.
- **C9 — Analytics Integration (1 point):** deferred optional stage.

The project is developed incrementally, using separate Git commits and completion tags for each coursework stage.
