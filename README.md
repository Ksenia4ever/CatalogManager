# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests, Postman API testing, Playwright browser-based E2E testing, and GitHub Actions automation.

The project was developed for the **Web Technologies** coursework.

## Completed Coursework Stages

- **C1 — MVC Application:** CRUD operations for three related entities with optional product image URLs.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation, and HTTP status codes.
- **QA1 — Automated Web API Testing:** automated positive and negative API tests using Postman.
- **B1 — Pagination API:** offset-based pagination using `skip`, `limit`, and `nextLink`.
- **C8 — External API Integration:** interactive stores map powered by OpenStreetMap and Leaflet.
- **B9 — API Output Caching:** in-memory response caching with expiration and tag-based invalidation.
- **C4 — Docker Containerization:** ASP.NET Core and PostgreSQL containers using Docker Compose.
- **C6 — Bootstrap Landing Page:** responsive product catalog and individual product landing pages.
- **C7 — SEO Optimization:** product-specific metadata, canonical URLs, Open Graph tags, robots.txt, and Lighthouse auditing.
- **F1 — Data Visualization:** statistical dashboard using Chart.js and PostgreSQL data.
- **QA2 — End-to-End Testing:** automated Chromium browser tests with Playwright, xUnit, and GitHub Actions.
- **F2 — AJAX Autocomplete:** server-side category and store autocomplete in product forms using Select2, jQuery, AJAX, and ASP.NET Core.

---

## Features

- **Categories:** create, view, edit, and delete categories; reject duplicate names.
- **Stores:** manage names, addresses, and geographic coordinates.
- **Products:** manage names, descriptions, prices, optional images, categories, and stores.
- **Validation:** required fields, valid prices, coordinates, and related entity IDs.
- **Deletion constraints:** prevent deleting referenced categories and stores.
- **MVC interface:** Razor Views, Bootstrap, ViewModels, and antiforgery validation.
- **REST API:** CRUD endpoints for categories, stores, and products.
- **Pagination:** offset-based pagination for API collections.
- **Stores Map:** interactive markers, popups, search, and refresh.
- **Output caching:** 30-second API caching with invalidation.
- **Docker:** containerized web application and PostgreSQL with persistent data.
- **Product landing pages:** Bootstrap product cards and individual details pages.
- **SEO:** metadata, canonical URLs, Open Graph, and robots.txt.
- **Statistics:** three Chart.js charts with asynchronous refresh.
- **Autocomplete:** dynamically search and select categories and stores when creating or editing products.
- **Automated testing:** xUnit, Moq, HTTP integration tests, Postman, and Playwright.
- **GitHub Actions:** automated QA2 execution against Docker containers.

---

## Technology Stack

- C# / .NET 10
- ASP.NET Core MVC, Web API, Razor Views
- Entity Framework Core 10
- PostgreSQL 17 and Npgsql
- Bootstrap, HTML, CSS, JavaScript
- jQuery and Select2
- AJAX and JSON
- Chart.js
- OpenStreetMap and Leaflet.js
- ASP.NET Core Output Caching
- Docker and Docker Compose
- xUnit and Moq
- WebApplicationFactory
- Microsoft Playwright for .NET
- Chromium
- Postman Collection Runner
- GitHub Actions
- Google Chrome Lighthouse
- Coverlet and ReportGenerator
- Git and GitHub

---

## Solution Structure

```text
CatalogManager/
├── CatalogManager.slnx
├── .github/
│   └── workflows/
│       ├── c3-ci-cd.yml
│       └── qa2-playwright.yml
├── Frontend/
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   ├── ProductAutocompleteController.cs
│   │   ├── StoresMapController.cs
│   │   └── StatisticsController.cs
│   ├── ApiControllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Views/
│   │   ├── Products/
│   │   │   ├── Index.cshtml
│   │   │   ├── Details.cshtml
│   │   │   ├── Create.cshtml
│   │   │   └── Edit.cshtml
│   │   ├── StoresMap/
│   │   ├── Statistics/
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   ├── wwwroot/
│   │   ├── js/
│   │   │   ├── stores-map.js
│   │   │   ├── statistics.js
│   │   │   └── product-autocomplete.js
│   │   ├── css/
│   │   │   ├── stores-map.css
│   │   │   ├── product-landing.css
│   │   │   ├── statistics.css
│   │   │   └── product-autocomplete.css
│   │   └── robots.txt
│   └── Program.cs
├── Application/
├── Domain/
├── Infrastructure/
├── UnitTests/
├── IntegrationTests/
│   └── Tests/
│       ├── StatisticsIntegrationTests.cs
│       └── ProductAutocompleteIntegrationTests.cs
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

The actual `.env` file contains local environment configuration and must not be committed.

---

## Architecture and Data Model

CatalogManager follows a layered architecture.

**MVC requests:**

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

**REST API requests:**

HTTP Client → API Controller → Application Service → Repository → EF Core / PostgreSQL

**F2 autocomplete requests:**

Select2 → AJAX GET → ProductAutocompleteController → EF Core → PostgreSQL → JSON → Select2

Each Product belongs to one Category and one Store.

Categories and stores may be associated with multiple products. Foreign-key constraints protect referenced records from deletion.

MVC uses dedicated form ViewModels:

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

REST API endpoints use separate request and response DTOs.

---

## C1 — MVC Application

MVC controllers `CategoriesController`, `StoresController`, and `ProductsController` provide Create, Read, Update, and Delete operations through Razor Views.

Validation is implemented using Data Annotations and application business rules.

Form POST actions use antiforgery validation.

The MVC interface provides the foundation for C6 product landing pages, QA2 browser testing, and F2 autocomplete.

---

## C5 — REST API

| Entity | Collection Endpoint | Resource Endpoint |
|---|---|---|
| Categories | `/api/categories` | `/api/categories/{id}` |
| Stores | `/api/stores` | `/api/stores/{id}` |
| Products | `/api/products` | `/api/products/{id}` |

Supported operations include GET, POST, PUT, and DELETE.

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

Category and store IDs must refer to existing database records.

---

## QA1 — Automated Postman API Testing

The original QA1 implementation contains 50 requests across four folders:

1. **Categories (13):** CRUD, duplicate names, invalid JSON, and validation.
2. **Stores (13):** CRUD, required fields, and coordinate validation.
3. **Products (15):** CRUD, price validation, foreign keys, and invalid JSON.
4. **Cleanup and Relationship Checks (9):** deletion conflicts, cleanup, and missing-resource checks.

### Automation

- JavaScript pre-request and post-response scripts.
- Assertions using `pm.test()` and `pm.expect()`.
- Dynamic identifiers stored in collection variables.
- Unique test data for each execution.
- Cleanup respecting foreign-key relationships.
- Positive and negative scenarios.

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
- `limit` — maximum number of returned records.
- `nextLink` — URL for the following page, when available.

Paginated responses contain:

- `items`
- `skip`
- `limit`
- `totalCount`
- `nextLink`

Requests without pagination parameters retain the original C5 JSON array response.

Pagination is performed at the database query level using Entity Framework Core.

Detailed documentation: [B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md).

---

## C8 — Interactive Stores Map

C8 integrates OpenStreetMap and Leaflet.js.

Page:

`/StoresMap`

Data endpoint:

```http
GET /api/stores
```

### Features

- Interactive OpenStreetMap map.
- Store markers with saved coordinates.
- Store information popups.
- Search by store name.
- Visible marker counter.
- Refresh without full page reload.
- Geographic coordinate validation.
- Navigation through the application menu.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | MVC controller |
| `Frontend/Views/StoresMap/Index.cshtml` | Map page |
| `Frontend/wwwroot/js/stores-map.js` | Map behavior |
| `Frontend/wwwroot/css/stores-map.css` | Styling |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests |

Three integration test cases verify the map page and related resources.

No additional database migrations were required.

---

## B9 — API Output Caching

B9 implements in-memory server-side response caching using ASP.NET Core Output Cache.

### Cache Policies

| Policy | Resource | Duration | Tag |
|---|---|---|---|
| `CategoriesCache` | Categories API | 30 seconds | `categories` |
| `StoresCache` | Stores API | 30 seconds | `stores` |
| `ProductsCache` | Products API | 30 seconds | `products` |

The application uses `AddOutputCache()` and `UseOutputCache()`.

GET responses may be cached for 30 seconds.

Successful API POST, PUT, and DELETE operations invalidate relevant cache tags through `IOutputCacheStore.EvictByTagAsync()`.

Direct PostgreSQL modifications and MVC changes are not automatically covered by API-controller cache invalidation.

### Verified B9 Results

The Postman collection contains 24 B9 requests.

| Metric | Result |
|---|---:|
| Requests | 24 |
| Assertions | 48 |
| Passed | 48 |
| Failed | 0 |
| Errors | 0 |

Verified on 29 October 2026.

These tests validate API responses and data visibility. An actual cache HIT would require additional server-side diagnostics.

### Limitations

- Cache entries belong to a single application instance.
- Cache entries expire after 30 seconds.
- Cache data is lost when the application process restarts.
- The cache is not shared across multiple containers.
- Direct database or MVC modifications may not invalidate cached responses immediately.

---

## C4 — Docker Containerization

C4 runs CatalogManager and PostgreSQL in separate Linux containers managed by Docker Compose.

### Docker Architecture

```text
Windows Host
    |
    | http://localhost:8085
    v
Docker Compose
    |
    +--- web container
    |    ASP.NET Core 10
    |    Port 8080
    |       |
    |       | db:5432
    |       v
    +--- db container
         PostgreSQL 17
         Port 5432
             |
             v
         Docker Volume
         catalogmanager_pgdata
```

The application is accessible at:

`http://localhost:8085`

The PostgreSQL database is exposed locally at:

`127.0.0.1:55432`

### Docker Files

| File | Purpose |
|---|---|
| `Dockerfile` | Multi-stage .NET build |
| `.dockerignore` | Build context exclusions |
| `compose.yaml` | Web and PostgreSQL services |
| `.env.example` | Example configuration |
| `.env` | Local configuration, excluded from Git |

### Start Docker Environment

Open Windows PowerShell in the solution root.

```powershell
docker compose up --build -d
```

Check containers:

```powershell
docker compose ps -a
```

Expected state:

- PostgreSQL container: Healthy.
- ASP.NET Core container: Up.

### Database Migrations

A new Docker database requires EF Core migrations.

Temporarily configure the Docker database connection in the current PowerShell session:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=55432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_DOCKER_PASSWORD"
```

Apply migrations:

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

Remove the override:

```powershell
Remove-Item Env:ConnectionStrings__DefaultConnection
```

### Useful Docker Commands

Restart:

```powershell
docker compose restart
```

Stop without deleting the database volume:

```powershell
docker compose down
```

Start again:

```powershell
docker compose up -d
```

View application logs:

```powershell
docker compose logs web --tail=80
```

Do not use `docker compose down -v` unless database deletion is intentional.

### Verified Results

- Docker Compose configuration validated.
- ASP.NET Core and PostgreSQL containers started.
- PostgreSQL health checks passed.
- EF Core migrations were applied.
- MVC and REST API requests worked.
- Database data survived container restarts.
- Docker was successfully reused for QA2 automated tests.

---

## C6 — Bootstrap Product Landing Pages

C6 improves product presentation using Bootstrap.

### Product Catalog

`/Products`

The catalog displays products using a responsive card layout.

### Individual Product Page

`/Products/Details/{id}`

Example:

`/Products/Details/1`

### Features

- Responsive product cards.
- Dedicated product landing pages.
- Product names and descriptions.
- Prices and optional images.
- Category and store information.
- Navigation between catalog and details.
- Custom CSS.
- Responsive desktop and mobile layouts.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Views/Products/Index.cshtml` | Product catalog |
| `Frontend/Views/Products/Details.cshtml` | Product details |
| `Frontend/wwwroot/css/product-landing.css` | Product styling |

Existing MVC controllers, services, and database tables are reused.

No new migrations were required.

---

## C7 — SEO Optimization

C7 improves technical SEO for C6 product landing pages.

### Features

- Product-specific HTML page titles.
- Meta descriptions.
- Canonical URLs.
- Open Graph metadata.
- robots.txt.
- Google Chrome Lighthouse SEO auditing.

### Robots.txt

File:

`Frontend/wwwroot/robots.txt`

Public path:

`/robots.txt`

Contents:

```text
User-agent: *
Allow: /
```

These rules permit crawling but do not guarantee indexing or ranking.

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

The result applies to the audited local product page.

---

## F1 — Data Visualization

F1 adds a statistical dashboard using Chart.js.

### Statistics Page

`/Statistics`

### Data Endpoint

```http
GET /Statistics/Data
```

The controller reads PostgreSQL records and produces aggregated JSON data.

### Summary Indicators

- Total products.
- Total categories.
- Total stores.

### Charts

1. **Products by Category:** distribution of products across categories.
2. **Products by Store:** number of products associated with each store.
3. **Price Distribution:** products grouped into price ranges.

### Features

- Automatic data loading.
- Asynchronous requests with Fetch API.
- Interactive Chart.js diagrams.
- Refresh functionality.
- Loading and error states.
- Responsive Bootstrap layout.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StatisticsController.cs` | Page and JSON endpoint |
| `Frontend/Views/Statistics/Index.cshtml` | Dashboard |
| `Frontend/wwwroot/js/statistics.js` | Data loading and charts |
| `Frontend/wwwroot/css/statistics.css` | Styling |
| `IntegrationTests/Tests/StatisticsIntegrationTests.cs` | Integration tests |

Four new integration tests were introduced.

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

QA2 introduces automated real-browser testing using:

- Microsoft Playwright for .NET.
- xUnit.
- Chromium.

Unlike unit and HTTP integration tests, E2E tests interact with the application as a user would.

They verify navigation, forms, validation, and selected application pages.

### Test Project

Project:

`E2ETests/CatalogManager.E2ETests.csproj`

Test class:

`E2ETests/CatalogManagerBrowserTests.cs`

### Implemented Scenarios

| Test | Scenario |
|---|---|
| QA2-01 | Home page and navigation to Products |
| QA2-02 | Complete Category Create → Edit → Delete workflow |
| QA2-03 | Required category name validation |
| QA2-04 | Product catalog and Create form |
| QA2-05 | Stores Map and search/refresh controls |
| QA2-06 | Statistics dashboard and refresh control |
| QA2-07 | robots.txt availability |

### Browser Configuration

The tests run Chromium in headless mode.

The target application URL is configured through:

`CATALOGMANAGER_E2E_BASE_URL`

Default URL:

`https://localhost:44361`

Docker URL:

`http://localhost:8085`

### Install Playwright Chromium

Build the test project:

```powershell
dotnet build E2ETests/CatalogManager.E2ETests.csproj
```

Install Chromium from Windows PowerShell:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\E2ETests\bin\Debug\net10.0\playwright.ps1" install chromium
```

### Run QA2 with IIS Express

1. Configure the local PostgreSQL database.
2. Open `CatalogManager.slnx` in Visual Studio.
3. Select IIS Express.
4. Start the application using Ctrl + F5.
5. Verify that `https://localhost:44361/Products/Create` is accessible.
6. Open Developer PowerShell.
7. Configure the target URL:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"
```

8. Run the tests:

```powershell
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

The application must remain running during E2E tests.

### Run QA2 with Docker

Start the application and database:

```powershell
docker compose up --build -d
```

Configure the E2E URL:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "http://localhost:8085"
```

Run the tests:

```powershell
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Verified Local QA2 Results

| Environment | Total | Passed | Failed | Skipped | Duration |
|---|---:|---:|---:|---:|---|
| Visual Studio + IIS Express | 7 | 7 | 0 | 0 | 9.7 seconds |
| Docker + PostgreSQL | 7 | 7 | 0 | 0 | 9.2 seconds |

Verified on 30 October 2026.

Later local Test Explorer executions returned `ERR_CONNECTION_REFUSED` when Playwright could not connect to the expected IIS Express URL. This is an application availability issue, rather than evidence that individual test assertions failed.

The Docker-based CI workflow provides an independently verified execution environment.

### E2E Functional Coverage Assessment

| Functional Area | Scenario | Coverage |
|---|---|---|
| Navigation | Home to Products | Covered |
| Categories | Create, Edit, Delete | Covered |
| Category validation | Required Name | Covered |
| Products | Catalog and Create form availability | Partially covered |
| Stores Map | Page and search/refresh controls | Partially covered |
| Statistics | Page and refresh control | Partially covered |
| SEO | robots.txt availability | Covered |
| Stores CRUD | Complete browser CRUD | Not covered |
| Products CRUD | Complete browser CRUD | Not covered |
| API negative scenarios | Validation and conflict responses | Covered separately by QA1 |

This is a functional scenario coverage assessment, not an estimate of source-code line coverage.

The Category E2E workflow creates a uniquely named test record and deletes it after successful execution.

Run these tests only against development or disposable test databases.

---

## GitHub Actions — Automated QA2 Testing

### Overview

QA2 is integrated with GitHub Actions.

The workflow automatically builds and runs the CatalogManager application in Docker, prepares PostgreSQL, installs Chromium, and executes Playwright browser tests.

This allows the E2E test suite to run independently of a developer's Visual Studio or IIS Express installation.

### Workflow File

`.github/workflows/qa2-playwright.yml`

### Triggers

The workflow is configured for:

- Pushes to `develop`.
- Pushes to `main`.
- Pull requests targeting these branches.
- Manual execution using `workflow_dispatch`.

### Automated Workflow

1. Checkout the GitHub repository.
2. Install the .NET 10 SDK.
3. Build and start Docker Compose services.
4. Verify container status.
5. Install the Entity Framework Core CLI.
6. Restore NuGet dependencies.
7. Apply existing PostgreSQL migrations.
8. Wait until the CatalogManager application becomes available.
9. Restore and build the E2E test project.
10. Install Playwright Chromium and required dependencies.
11. Run the seven QA2 tests.
12. Upload the test results as a GitHub Actions artifact.

### CI Environment

GitHub Actions uses an Ubuntu runner.

The application and PostgreSQL run in Docker containers.

Playwright executes on the GitHub Actions runner and connects to:

`http://localhost:8085`

The CI database uses isolated, temporary configuration. Its test credentials are not the local development database credentials.

### Verified GitHub Actions Result

**Verification date:** 31 October 2026.

| Metric | Result |
|---|---|
| Workflow | QA2 - Playwright E2E Tests |
| Job | QA2 - Docker and Chromium |
| Branch | `develop` |
| Status | **Success** |
| E2E suite | 7 automated tests |
| Artifact | `qa2-playwright-results` |
| Total workflow duration | Approximately 2 minutes |

The successful workflow run confirms that the QA2 automation completed on GitHub-hosted infrastructure.

The detailed test report is available in the workflow logs and uploaded artifact.

### How to Verify QA2 on GitHub

1. Open the CatalogManager repository.
2. Select the **Actions** tab.
3. Open **QA2 - Playwright E2E Tests**.
4. Select the latest successful workflow run.
5. Open the job **QA2 - Docker and Chromium**.
6. Expand **Run seven QA2 tests** to view the test execution output.
7. Download `qa2-playwright-results` if the detailed TRX test report is required.

A successful workflow run provides evidence that the project can be tested automatically without starting the application manually on the reviewer's computer.

### Important Notes

- Docker and PostgreSQL are started automatically inside GitHub Actions.
- Playwright Chromium is installed automatically.
- The test runner uses the configured Docker application URL.
- Local IIS Express is not required for CI testing.
- GitHub Actions displays test job success or failure.
- CI results reflect the source code committed in the corresponding Git revision.
- GitHub Actions provides automated testing, not cloud deployment of the application.

---

## F2 — AJAX Autocomplete

### Overview

F2 improves product creation and editing by replacing ordinary category and store dropdown interactions with asynchronous autocomplete selectors.

The implementation uses:

- JavaScript.
- jQuery.
- Select2.
- AJAX requests.
- ASP.NET Core MVC.
- Entity Framework Core.
- PostgreSQL.

The goal is to allow users to find categories and stores without manually scrolling through long dropdown lists.

### Product Forms

Autocomplete is available on:

`/Products/Create`

`/Products/Edit/{id}`

The Category and Store fields support text-based searching and selection.

### Autocomplete Behavior

- Search begins after the user enters at least three characters.
- AJAX requests are delayed by approximately 300 milliseconds after typing.
- The frontend requests matching records from the backend.
- The backend filters results using PostgreSQL.
- The response contains a limited number of matching items.
- The user selects the required category or store.
- The selected identifiers are submitted using the existing product form.

The backend returns no more than 20 matching options for a search request.

### Example User Scenario

A user wants to create a new product.

Instead of opening a long Category dropdown, the user enters:

`Ele`

The autocomplete selector sends a request to the backend.

The server searches for matching category names and returns the relevant records.

The user selects the correct category.

The same workflow is available for the Store field.

### AJAX Architecture

```text
User types in Category / Store field
            |
            v
Select2 + jQuery
            |
            | AJAX request
            v
ProductAutocompleteController
            |
            v
Entity Framework Core
            |
            v
PostgreSQL
            |
            | Matching records
            v
JSON response
            |
            v
Autocomplete results in browser
```

The server performs the search instead of sending the entire matching dataset to the browser for client-side filtering.

### Implemented Features

- AJAX autocomplete for categories.
- AJAX autocomplete for stores.
- Minimum input length of three characters.
- Configurable request delay.
- Server-side filtering.
- Limited search results.
- Integration with existing MVC product forms.
- Selected Category and Store IDs preserved in form submission.
- Compatibility with product creation and editing.
- Existing validation and antiforgery behavior preserved.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/ProductAutocompleteController.cs` | Autocomplete endpoints and database search |
| `Frontend/wwwroot/js/product-autocomplete.js` | Select2 initialization and AJAX behavior |
| `Frontend/wwwroot/css/product-autocomplete.css` | Autocomplete styling |
| `Frontend/Views/Products/Create.cshtml` | Product creation with autocomplete |
| `Frontend/Views/Products/Edit.cshtml` | Product editing with autocomplete |
| `IntegrationTests/Tests/ProductAutocompleteIntegrationTests.cs` | Automated F2 integration tests |

### Testing in Visual Studio

F2 was tested using Visual Studio and IIS Express, without requiring Docker.

Verification included:

1. Building the updated solution.
2. Running the existing unit tests.
3. Running the HTTP integration tests.
4. Opening the product Create and Edit forms.
5. Searching categories and stores through the autocomplete selectors.
6. Confirming that the selections work in the existing product forms.
7. Inspecting browser AJAX behavior.

Browser Developer Tools can be used to demonstrate the requests:

**Chrome → F12 → Network → Fetch/XHR**

### Automated F2 Tests

Six additional HTTP integration test cases were introduced.

The tests cover:

- Short search terms.
- Search terms containing at least three characters.
- Product forms using autocomplete.

### Verified F2 Results

| Test Type | Before F2 | After F2 |
|---|---:|---:|
| Unit Tests | 208 | 208 |
| Integration Tests | 59 | 65 |
| **Total** | **267** | **273** |

**Result: 273 passed unit/integration tests, 0 failed.**

The six additional integration tests passed in Visual Studio.

Manual browser verification also confirmed that the autocomplete feature works.

### Compatibility

F2 extends the existing product editing interface without changing the product database schema.

Existing entities, application services, and CRUD operations are reused.

No new Entity Framework Core migration was required.

The previous QA2 suite remains available and is executed independently through GitHub Actions.

### Limitations

- Autocomplete requires JavaScript and the Select2 library.
- AJAX requests require access to the running ASP.NET Core backend.
- Server-side filtering limits returned results, but actual performance with millions of records has not been benchmarked.
- Large-scale production use may require additional indexing and query optimization.
- Existing form-loading logic may still retrieve full option lists internally; this is a potential future optimization.

### F2 Conclusion

F2 improves the usability of the CatalogManager product forms.

Users can search for categories and stores using AJAX autocomplete instead of manually navigating long dropdown lists.

The implementation reuses the existing MVC application and PostgreSQL data model.

**F2 result: implemented, browser-verified, and six new integration tests passed.**

---

## Postman Collection — QA1 + B1 + B9

Collection file:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

The collection includes tests for:

- QA1 automated REST API testing.
- B1 pagination.
- B9 API output caching.

### Import and Run

1. Start CatalogManager.
2. Import the collection into Postman.
3. Set the `baseUrl` collection variable.
4. Choose the relevant folder.
5. Run it through Postman Collection Runner.
6. Review Passed, Failed, and Errors.

IIS Express URL:

`https://localhost:44361`

Docker URL:

`http://localhost:8085`

Run data-changing tests only against development or test databases.

---

## Local Setup Without Docker

### 1. Restore Dependencies

```powershell
dotnet restore CatalogManager.slnx
```

### 2. Configure PostgreSQL

Create the database:

`CatalogManagerDb`

Configure the connection with User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with the local database password.

### 3. Apply Migrations

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

### 4. Run the Application

Open the solution in Visual Studio and use the IIS Express launch profile.

Alternatively, launch the application using the .NET CLI:

```powershell
dotnet run --project Frontend
```

The URL depends on the selected development profile.

---

## Automated Testing

The solution includes three test projects.

| Project | Purpose |
|---|---|
| `UnitTests` | Unit testing using xUnit and Moq |
| `IntegrationTests` | HTTP integration testing using WebApplicationFactory and PostgreSQL |
| `E2ETests` | Browser-based testing using Playwright and Chromium |

### Integration Test Database

The HTTP integration tests use a separate database:

`CatalogManagerTestDb`

Configure it through User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

### Run Unit Tests

```powershell
dotnet test UnitTests
```

### Run Integration Tests

```powershell
dotnet test IntegrationTests
```

### Run E2E Tests with IIS Express

Keep CatalogManager running at:

`https://localhost:44361`

Then execute:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Run E2E Tests with Docker

Keep CatalogManager running at:

`http://localhost:8085`

Then execute:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "http://localhost:8085"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Run E2E Tests Automatically on GitHub

Open:

**GitHub Repository → Actions → QA2 - Playwright E2E Tests**

The workflow automatically prepares Docker, PostgreSQL, the application, Playwright, and Chromium.

### Latest Confirmed Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 65 |
| Playwright E2E Tests | 7 |
| **Total** | **280** |

**280 tests passed across the separately verified test suites.**

- The 273 unit/integration tests passed after implementing F2.
- The QA2 GitHub Actions workflow completed successfully.
- Tests were executed in separate environments rather than one single local Test Explorer run.

---

## Code Coverage

Collect C# code coverage:

```powershell
dotnet test CatalogManager.slnx --collect:"XPlat Code Coverage"
```

Install ReportGenerator if necessary:

```powershell
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate the combined report:

```powershell
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open:

`CoverageReport/index.html`

The previously reviewed pre-C5 coverage report showed:

- Line coverage: 76.1%.
- Branch coverage: 77.7%.
- Application layer line coverage: 100%.

These are historical measurements and are not claimed as current F2 coverage results.

QA2's functional scenario coverage is documented separately.

---

## Security Notes

- Never commit PostgreSQL passwords, API tokens, or secret connection strings.
- Keep local `.env` files excluded from Git.
- Use `.env.example` for configuration documentation.
- Use User Secrets for local development credentials.
- Use GitHub Secrets for sensitive CI configuration when needed.
- Run data-changing tests only against development or test databases.
- Check exported Postman collections for credentials.
- MVC forms use antiforgery protection.
- REST API inputs use DTO validation and business rules.
- Docker PostgreSQL is bound to localhost.
- `robots.txt` is not an access-control mechanism.
- Public deployment would require further security hardening.

---

## Project Status

### Implemented and Verified

- **C1 — MVC Application:** implemented; video defense recorded.
- **C5 — REST API:** implemented and tested.
- **QA1 — Automated Web API Testing:** 62/62 and 124/124 assertions passed.
- **B1 — Pagination API:** implemented and verified.
- **C8 — Interactive Stores Map:** implemented and verified.
- **B9 — API Output Caching:** 48/48 Postman assertions passed.
- **C4 — Docker Containerization:** verified with PostgreSQL 17 and persistent data.
- **C6 — Bootstrap Landing Page:** implemented and verified.
- **C7 — SEO Optimization:** Lighthouse SEO score of 100/100.
- **F1 — Data Visualization:** implemented with three Chart.js diagrams.
- **QA2 — End-to-End Testing:** seven Playwright tests; GitHub Actions workflow successfully executed.
- **F2 — AJAX Autocomplete:** implemented and browser-verified; six new integration tests passed.

### Latest Testing Summary

- **208** unit tests passed.
- **65** HTTP integration tests passed.
- **7** Playwright E2E tests passed through automated GitHub Actions.
- **280** total passing tests across the confirmed runs.

### Remaining Tasks

- Verify that no local `.env` files are tracked in Git.
- Independently verify actual B9 Cache HIT using server-side diagnostics.
- Commit the F2 implementation and updated README.
- Implement and verify B2.
- Prepare a combined F1, QA2, F2, and B2 video defense.

### Planned Next Stages

- **B2 — External API Integration (2 points):** integrate currency exchange information through an additional external API.
- **C9 — Analytics Integration (1 point):** deferred optional stage.

After completing B2, prepare a combined defense of four coursework stages:

**F1 + QA2 + F2 + B2**

The project is developed incrementally using Git commits, completion tags, automated tests, and GitHub Actions.