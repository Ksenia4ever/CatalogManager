
# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes a responsive interface, REST API, interactive map, pagination, caching, data visualization, AJAX autocomplete, currency exchange rates, and a dynamic notes editor.

The project includes automated unit, integration, Postman API, and Playwright E2E tests, as well as Docker containerization and GitHub Actions automation.

The project was developed for the **Web Technologies** coursework.

## Completed Coursework Stages

- **C1 — MVC Application (3 points):** CRUD operations for three related entities with optional product images.
- **C5 — REST API (3 points):** JSON CRUD endpoints, DTOs, validation, and HTTP status codes.
- **QA1 — Automated Web API Testing (2 points):** positive and negative API testing using Postman.
- **B1 — Pagination API (2 points):** offset pagination with `skip`, `limit`, and `nextLink`.
- **C8 — External API Integration (3 points):** interactive stores map with OpenStreetMap and Leaflet.
- **B9 — API Output Caching (3 points):** 30-second in-memory caching with invalidation.
- **C4 — Docker Containerization (1 point):** ASP.NET Core and PostgreSQL services using Docker Compose.
- **C6 — Bootstrap Landing Page (3 points):** responsive catalog and individual product landing pages.
- **C7 — SEO Optimization (1 point):** metadata, canonical URLs, robots.txt, and Lighthouse audits.
- **F1 — Data Visualization (2 points):** Chart.js statistical dashboard.
- **QA2 — End-to-End Testing (3 points):** Chromium browser tests with Playwright and GitHub Actions.
- **F2 — AJAX Autocomplete (4 points):** server-side category and store search using Select2.
- **B2 — External API Integration (2 points):** PrivatBank exchange rates and currency converter.
- **C12 — Dynamic Notes Editor (3 points):** browser-based block editor with automatic localStorage persistence.

**Total implemented coursework stages: 14.**

**Total potential coursework points: 35.**

These points represent implemented coursework tasks, not an official grade awarded by the instructor.

---

## Features

### Catalog Management

- Create, read, update, and delete categories.
- Create, read, update, and delete stores.
- Create, read, update, and delete products.
- Associate products with categories and stores.
- Validate prices, required fields, and geographic coordinates.
- Prevent deletion of referenced categories and stores.
- Prevent duplicate category names.
- Support optional product image URLs.

### Web Interface

- Responsive Bootstrap and Razor Views.
- Product catalog with individual product landing pages.
- Interactive stores map using Leaflet.
- Statistics dashboard using Chart.js.
- AJAX autocomplete for category and store selection.
- Currency exchange rates from PrivatBank.
- Approximate currency conversion.
- Dynamic block-based notes editor.

### Backend and Infrastructure

- REST API with CRUD operations.
- Offset-based pagination.
- Server-side output caching.
- PostgreSQL persistence.
- Entity Framework Core migrations.
- Docker Compose support.
- Dependency Injection and HttpClient integration.

### Testing

- xUnit unit tests.
- HTTP integration tests using WebApplicationFactory.
- Postman automated API tests.
- Playwright browser-based E2E tests.
- GitHub Actions automated QA2 workflow.

---

## Technology Stack

### Backend

- C# / .NET 10
- ASP.NET Core MVC
- ASP.NET Core Web API
- Razor Views
- Entity Framework Core 10
- PostgreSQL 17
- Npgsql
- ASP.NET Core Output Caching
- HttpClient / IHttpClientFactory
- Dependency Injection
- System.Text.Json

### Frontend

- HTML5
- CSS3
- JavaScript
- Bootstrap
- jQuery
- Select2
- AJAX / Fetch API
- Chart.js
- Leaflet.js
- Web Storage API / localStorage
- Dynamic DOM manipulation

### External Services

- OpenStreetMap
- PrivatBank Public API

### Testing and Development

- Visual Studio
- xUnit
- Moq
- WebApplicationFactory
- Microsoft Playwright
- Chromium
- Postman
- Docker and Docker Compose
- GitHub Actions
- Coverlet
- ReportGenerator
- Google Chrome Lighthouse
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
│   │   ├── CategoriesController.cs
│   │   ├── StoresController.cs
│   │   ├── ProductsController.cs
│   │   ├── StoresMapController.cs
│   │   ├── StatisticsController.cs
│   │   ├── ProductAutocompleteController.cs
│   │   ├── CurrencyRatesController.cs
│   │   └── NotesEditorController.cs
│   ├── ApiControllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Services/
│   │   ├── ICurrencyRatesService.cs
│   │   └── PrivatBankCurrencyRatesService.cs
│   ├── Views/
│   │   ├── Categories/
│   │   ├── Stores/
│   │   ├── Products/
│   │   │   ├── Index.cshtml
│   │   │   ├── Details.cshtml
│   │   │   ├── Create.cshtml
│   │   │   └── Edit.cshtml
│   │   ├── StoresMap/
│   │   │   └── Index.cshtml
│   │   ├── Statistics/
│   │   │   └── Index.cshtml
│   │   ├── CurrencyRates/
│   │   │   └── Index.cshtml
│   │   ├── NotesEditor/
│   │   │   └── Index.cshtml
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   ├── wwwroot/
│   │   ├── js/
│   │   │   ├── stores-map.js
│   │   │   ├── statistics.js
│   │   │   ├── product-autocomplete.js
│   │   │   ├── currency-rates.js
│   │   │   └── notes-editor.js
│   │   ├── css/
│   │   │   ├── stores-map.css
│   │   │   ├── product-landing.css
│   │   │   ├── statistics.css
│   │   │   ├── product-autocomplete.css
│   │   │   └── notes-editor.css
│   │   └── robots.txt
│   └── Program.cs
├── Application/
├── Domain/
├── Infrastructure/
├── UnitTests/
├── IntegrationTests/
│   └── Tests/
│       ├── StatisticsIntegrationTests.cs
│       ├── ProductAutocompleteIntegrationTests.cs
│       └── PrivatBankCurrencyRatesServiceTests.cs
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

This tree highlights the main project files and coursework components. Additional controllers, views, models, and tests may also exist.

The local `.env` file must not be committed to Git.

---

## Architecture and Data Model

CatalogManager uses a layered architecture.

### MVC Request Flow

```text
Browser
  ↓
MVC Controller
  ↓
Application Service
  ↓
Repository
  ↓
Entity Framework Core
  ↓
PostgreSQL
```

### REST API Request Flow

```text
HTTP Client
  ↓
API Controller
  ↓
Application Service
  ↓
Repository
  ↓
Entity Framework Core
  ↓
PostgreSQL
```

### Main Entities

**Category**

Stores category information. Category names are checked for duplicates.

**Store**

Stores the name, address, latitude, and longitude of a store.

**Product**

Stores the name, description, price, optional image URL, CategoryId, and StoreId.

Each product belongs to one category and one store.

Categories and stores referenced by existing products cannot be deleted.

### MVC ViewModels

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

The REST API uses separate DTOs.

---

## C1 — MVC Application

C1 implements CRUD operations for Categories, Stores, and Products using ASP.NET Core MVC and Razor Views.

Features include:

- Create, Read, Update, and Delete.
- Model validation.
- Application business rules.
- Antiforgery validation.
- Related entity selection.
- PostgreSQL persistence.

The MVC application provides the foundation for later coursework stages.

---

## C5 — REST API

The REST API provides JSON CRUD operations.

| Entity | Collection Endpoint | Item Endpoint |
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

- `200 OK` — successful read.
- `201 Created` — successful creation.
- `204 No Content` — successful update or deletion.
- `400 Bad Request` — invalid input.
- `404 Not Found` — missing resource.
- `409 Conflict` — business rule violation.

### Example Product Request

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

The category and store IDs must reference existing database records.

---

## QA1 — Automated Postman API Testing

QA1 includes automated positive and negative API test scenarios using Postman.

The original QA1 collection contains 50 requests in four folders:

1. Categories — 13 requests.
2. Stores — 13 requests.
3. Products — 15 requests.
4. Cleanup and Relationship Checks — 9 requests.

### Automation

- JavaScript pre-request scripts.
- Automated response assertions.
- Dynamic test identifiers.
- Collection variables.
- Unique test data.
- Positive and negative cases.
- Cleanup respecting foreign-key relationships.

### Verified Results

| Iterations | Passed Assertions | Failed |
|---|---:|---:|
| 1 | 62 | 0 |
| 2 | 124 | 0 |

Verified on 28 October 2026.

---

## B1 — Pagination API

B1 implements offset-based pagination.

### Example Requests

```http
GET /api/categories?skip=0&limit=10
GET /api/stores?skip=0&limit=10
GET /api/products?skip=0&limit=10
```

Pagination parameters:

- `skip`: number of records skipped.
- `limit`: maximum records returned.
- `nextLink`: link to the next page, if available.

Paginated responses contain:

- `items`
- `skip`
- `limit`
- `totalCount`
- `nextLink`

Queries without pagination parameters retain the original C5 array response.

Pagination is performed at the database query level through Entity Framework Core.

Additional documentation:

[B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md)

---

## C8 — Interactive Stores Map

C8 integrates OpenStreetMap using Leaflet.js.

### Page

`/StoresMap`

### Data Source

```http
GET /api/stores
```

### Features

- OpenStreetMap tiles.
- Markers for stores with coordinates.
- Store information popups.
- Store search.
- Visible marker counter.
- Refresh without reloading the page.
- Coordinate validation.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | Controller |
| `Frontend/Views/StoresMap/Index.cshtml` | Razor View |
| `Frontend/wwwroot/js/stores-map.js` | JavaScript |
| `Frontend/wwwroot/css/stores-map.css` | CSS |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests |

Three C8 integration test cases verify the page and resources.

---

## B9 — API Output Caching

B9 uses ASP.NET Core Output Cache to cache eligible API GET responses.

### Policies

| Policy | API | Duration | Tag |
|---|---|---|---|
| `CategoriesCache` | Categories | 30 seconds | `categories` |
| `StoresCache` | Stores | 30 seconds | `stores` |
| `ProductsCache` | Products | 30 seconds | `products` |

The application uses:

- `AddOutputCache()`
- `UseOutputCache()`
- `IOutputCacheStore.EvictByTagAsync()`

Successful API modifications invalidate relevant cache tags.

Direct database and MVC changes are not automatically covered by API-controller invalidation.

### Postman Results

| Metric | Result |
|---|---:|
| Requests | 24 |
| Assertions | 48 |
| Passed | 48 |
| Failed | 0 |

Verified on 29 October 2026.

These tests verify API behavior and data visibility. They do not independently establish a server-side cache HIT.

### Limitations

- In-memory caching is instance-local.
- Cache entries expire after 30 seconds.
- Cache data is lost on application restart.
- Cache entries are not shared among independent application instances.

---

## C4 — Docker Containerization

C4 runs CatalogManager and PostgreSQL using Docker Compose.

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
    |       v
    +--- db
         PostgreSQL 17
         Port 5432
             |
             v
         Persistent Docker Volume
```

Application URL:

`http://localhost:8085`

Local database port:

`127.0.0.1:55432`

### Files

- `Dockerfile`
- `compose.yaml`
- `.dockerignore`
- `.env.example`

### Start Containers

```powershell
docker compose up --build -d
```

### Check Status

```powershell
docker compose ps -a
```

Expected state:

- PostgreSQL: Healthy.
- Application: Up.

### Database Migrations

A new database requires applying existing EF Core migrations.

Configure a temporary connection string for the Docker database:

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

### Additional Commands

```powershell
docker compose restart
docker compose logs web --tail=80
docker compose down
docker compose up -d
```

Do not use `docker compose down -v` unless deleting the persistent database volume is intentional.

### Verified Results

- Docker containers started successfully.
- PostgreSQL health check passed.
- Database migrations were applied.
- MVC and API requests worked.
- Data survived container restarts.
- Docker was reused for QA2.

---

## C6 — Bootstrap Product Landing Pages

C6 provides responsive product catalog pages.

### Catalog

`/Products`

### Product Details

`/Products/Details/{id}`

### Features

- Responsive product cards.
- Individual product pages.
- Names, descriptions, and prices.
- Optional images.
- Category and store information.
- Navigation between catalog and details.
- Custom CSS.

### Implementation Files

| File | Purpose |
|---|---|
| `Frontend/Views/Products/Index.cshtml` | Catalog |
| `Frontend/Views/Products/Details.cshtml` | Product details |
| `Frontend/wwwroot/css/product-landing.css` | Styling |

The existing application services and database schema are reused.

---

## C7 — SEO Optimization

C7 improves the technical SEO of product landing pages.

### Features

- Product-specific page titles.
- Meta descriptions.
- Canonical URLs.
- Open Graph metadata.
- robots.txt.
- Google Chrome Lighthouse auditing.

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

### Lighthouse Result

| Metric | Result |
|---|---:|
| SEO | 100/100 |
| Passed Audits | 9 |
| Not Applicable | 1 |
| Failed Applicable Audits | 0 |

The audit was performed on a local product details page on 30 October 2026.

The score does not guarantee search engine ranking.

---

## F1 — Data Visualization

F1 adds a statistical dashboard with Chart.js.

### Statistics Page

`/Statistics`

### JSON Endpoint

```http
GET /Statistics/Data
```

### Summary Indicators

- Total products.
- Total categories.
- Total stores.

### Charts

1. Products by Category.
2. Products by Store.
3. Product Price Distribution.

### Features

- Fetch API requests.
- Chart.js rendering.
- Interactive graphs.
- Refresh button.
- Error handling.
- Responsive layout.

### Implementation Files

| File | Purpose |
|---|---|
| `Frontend/Controllers/StatisticsController.cs` | Page and JSON endpoint |
| `Frontend/Views/Statistics/Index.cshtml` | Dashboard |
| `Frontend/wwwroot/js/statistics.js` | Charts |
| `Frontend/wwwroot/css/statistics.css` | Styling |
| `IntegrationTests/Tests/StatisticsIntegrationTests.cs` | Tests |

Four new integration tests were added.

### Verified Results After F1

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 59 |
| Total | 267 |

Verified on 30 October 2026.

---

## QA2 — End-to-End Testing with Playwright

QA2 introduces browser-based E2E testing with Microsoft Playwright, Chromium, and xUnit.

### Project

`E2ETests/CatalogManager.E2ETests.csproj`

### Main Test Class

`E2ETests/CatalogManagerBrowserTests.cs`

### Implemented Scenarios

| ID | Scenario |
|---|---|
| QA2-01 | Home page and navigation to Products |
| QA2-02 | Category Create, Edit, and Delete |
| QA2-03 | Required Category Name validation |
| QA2-04 | Product catalog and Create form |
| QA2-05 | Stores Map |
| QA2-06 | Statistics dashboard and refresh |
| QA2-07 | robots.txt |

### Test Configuration

The tests use:

`CATALOGMANAGER_E2E_BASE_URL`

Default URL:

`https://localhost:44361`

Docker URL:

`http://localhost:8085`

### Install Playwright

Build:

```powershell
dotnet build E2ETests/CatalogManager.E2ETests.csproj
```

Install Chromium on Windows:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\E2ETests\bin\Debug\net10.0\playwright.ps1" install chromium
```

### Run with Visual Studio

1. Start CatalogManager using IIS Express.
2. Verify that `https://localhost:44361/Products/Create` works.
3. Open Developer PowerShell.
4. Run:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Run Against Docker

```powershell
docker compose up --build -d
$env:CATALOGMANAGER_E2E_BASE_URL = "http://localhost:8085"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Verified Local Results

| Environment | Total | Passed | Failed | Skipped |
|---|---:|---:|---:|---:|
| IIS Express | 7 | 7 | 0 | 0 |
| Docker | 7 | 7 | 0 | 0 |

The application must be running before the E2E tests start.

### Functional Coverage Assessment

| Functional Area | Coverage |
|---|---|
| Navigation | Covered |
| Category CRUD | Covered |
| Category required-field validation | Covered |
| Product catalog and Create form | Partially covered |
| Stores Map | Partially covered |
| Statistics | Partially covered |
| robots.txt | Covered |
| Full Store CRUD through browser | Not covered |
| Full Product CRUD through browser | Not covered |

This assessment describes selected end-user workflows, not line coverage.

QA2 is designed for development or disposable test databases.

---

## GitHub Actions — QA2 Automation

QA2 is also executed automatically using GitHub Actions and Docker.

### Workflow File

`.github/workflows/qa2-playwright.yml`

### Triggers

- Push to `develop`.
- Push to `main`.
- Pull requests targeting supported branches.
- Manual workflow execution.

### Workflow Steps

1. Checkout repository.
2. Install .NET 10.
3. Build and start Docker Compose services.
4. Check PostgreSQL and application containers.
5. Restore NuGet dependencies.
6. Install EF Core tools.
7. Apply database migrations.
8. Wait for the application to become available.
9. Build E2E tests.
10. Install Playwright Chromium.
11. Run QA2 tests.
12. Upload test results.

### Environment

The GitHub runner uses Ubuntu.

The web application and PostgreSQL run inside Docker.

Playwright runs against:

`http://localhost:8085`

### Verified Result

| Metric | Result |
|---|---|
| Workflow | QA2 - Playwright E2E Tests |
| Job | QA2 - Docker and Chromium |
| Branch | develop |
| Status | Success |
| Test Scenarios | 7 |
| Artifact | qa2-playwright-results |

The successful workflow was verified on 31 October 2026.

### How to Verify

Open:

**GitHub Repository → Actions → QA2 - Playwright E2E Tests**

Select a successful workflow run and open:

**QA2 - Docker and Chromium → Run seven QA2 tests**

The uploaded TRX test results are available as a workflow artifact.

GitHub Actions provides reproducible testing; it does not deploy the application to a public cloud server.

---

## F2 — AJAX Autocomplete

F2 replaces ordinary Category and Store dropdown interactions in the Product forms with Select2-based asynchronous autocomplete.

### Product Forms

- `/Products/Create`
- `/Products/Edit/{id}`

### Behavior

- Search begins after three characters.
- AJAX delay is approximately 300 milliseconds.
- The backend filters matching categories and stores.
- PostgreSQL performs server-side search.
- At most 20 matches are returned.
- Selected IDs are submitted using the existing MVC forms.

### Architecture

```text
User types search text
       |
       v
Select2 / jQuery
       |
       v
AJAX request
       |
       v
ProductAutocompleteController
       |
       v
Entity Framework Core
       |
       v
PostgreSQL
       |
       v
JSON results
       |
       v
Select2 dropdown
```

### Files

| File | Purpose |
|---|---|
| `Frontend/Controllers/ProductAutocompleteController.cs` | Autocomplete endpoints |
| `Frontend/wwwroot/js/product-autocomplete.js` | Select2 / AJAX |
| `Frontend/wwwroot/css/product-autocomplete.css` | Styling |
| `Frontend/Views/Products/Create.cshtml` | Create form |
| `Frontend/Views/Products/Edit.cshtml` | Edit form |
| `IntegrationTests/Tests/ProductAutocompleteIntegrationTests.cs` | Tests |

### Testing

F2 was checked through Visual Studio Test Explorer and the browser.

The six new integration test cases cover:

- Short search terms.
- Searches containing at least three characters.
- Product forms with autocomplete.

### Verified Results

| Test Type | Before F2 | After F2 |
|---|---:|---:|
| Unit Tests | 208 | 208 |
| Integration Tests | 59 | 65 |
| Total | 267 | 273 |

Manual browser verification also confirmed that autocomplete functions correctly.

No database migration was required.

---

## B2 — PrivatBank External API Integration

B2 integrates CatalogManager with the public PrivatBank currency exchange API.

### Provider

PrivatBank

Base address:

`https://api.privatbank.ua/`

Endpoint:

```http
GET /p24api/pubinfo?json&exchange&coursid=5
```

### Pages

- `/CurrencyRates`
- `/CurrencyRates/Index`

### Features

- Server-side requests using HttpClient.
- JSON parsing using System.Text.Json.
- Display of bank purchase and selling rates.
- Refresh button.
- Approximate currency conversion.
- Handling of HTTP failures and malformed JSON.
- Automated tests using controlled HTTP responses.

### Currency Converter

The converter accepts an amount in UAH and a target currency.

Formula:

```text
Foreign currency amount = UAH amount / Bank selling rate
```

Example verified during testing:

```text
4000 UAH / 51.30 = 77.97 EUR
```

The result is indicative and is not a guaranteed bank transaction quote.

### Dependency Injection

`Frontend/Program.cs` registers a typed HttpClient:

```csharp
builder.Services.AddHttpClient<
    ICurrencyRatesService,
    PrivatBankCurrencyRatesService>(client =>
{
    client.BaseAddress = new Uri("https://api.privatbank.ua/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
```

### Architecture

```text
Browser
    |
    v
CurrencyRatesController
    |
    v
ICurrencyRatesService
    |
    v
PrivatBankCurrencyRatesService
    |
    v
HttpClient
    |
    v
PrivatBank Public API
    |
    v
JSON response
    |
    v
Currency Rate data
    |
    v
Browser
```

### Implementation Files

| File | Purpose |
|---|---|
| `Frontend/Services/ICurrencyRatesService.cs` | Service interface |
| `Frontend/Services/PrivatBankCurrencyRatesService.cs` | HTTP and JSON logic |
| `Frontend/Controllers/CurrencyRatesController.cs` | MVC controller |
| `Frontend/Views/CurrencyRates/Index.cshtml` | Currency rates View |
| `Frontend/wwwroot/js/currency-rates.js` | Refresh and converter |
| `Frontend/Program.cs` | HttpClient registration |
| `Frontend/Views/Shared/_Layout.cshtml` | Navigation |
| `IntegrationTests/Tests/PrivatBankCurrencyRatesServiceTests.cs` | Tests |

### Automated Tests

| Test | Purpose |
|---|---|
| `ValidResponse_ParsesCurrencyRates` | Valid JSON |
| `ServerError_ThrowsHttpRequestException` | HTTP failure |
| `NoValidRates_ThrowsJsonException` | No valid rates |
| `MalformedJson_ThrowsJsonException` | Malformed JSON |

The malformed JSON test uses `Assert.ThrowsAnyAsync<JsonException>()` to allow derived exception types.

### Verified Results

- Four B2 service tests passed.
- Currency Rates page opened successfully.
- EUR and USD rates were displayed.
- Approximate converter was verified in the browser.
- No database migration was required.

### Limitations

- Requires internet access.
- Depends on the external provider.
- Rates may change.
- Available currencies depend on the API response.
- Estimates are not financial transaction quotations.

---

## C12 — Dynamic Notes Editor

### Overview

C12 introduces a dynamic block-based notes editor into CatalogManager.

The editor allows a user to create and modify a document directly in the browser.

The implementation uses JavaScript, DOM manipulation, and the browser's localStorage API.

Unlike the existing catalog data, C12 notes are not stored in PostgreSQL.

### Editor Page

`/NotesEditor`

Alternative MVC route:

`/NotesEditor/Index`

The page is integrated into the ASP.NET Core MVC application and can be opened using the navigation menu or a direct URL.

### Supported Block Types

The editor supports:

1. **Heading** — for document headings.
2. **Paragraph** — for regular text.
3. **Bulleted List** — for unordered list content.
4. **Numbered List** — for ordered list content.

Blocks represent separate editable sections of the document.

### Implemented Features

- Create a document using editable blocks.
- Add headings.
- Add paragraphs.
- Add bulleted lists.
- Add numbered lists.
- Edit text inside existing blocks.
- Remove unwanted blocks.
- Move blocks up or down.
- Change the order of the document.
- Show formatted document content.
- Save changes automatically.
- Restore previously saved content when reopening the editor.
- Maintain the document using a dynamic JavaScript block list.

### Dynamic Block Model

Each document is represented in JavaScript as a collection of blocks.

Conceptually, a block contains:

- A block identifier.
- A block type.
- Text content.

The block list determines the document order.

When the user adds, modifies, deletes, or moves blocks, JavaScript updates the current document state and rerenders the relevant interface.

### C12 Architecture

```text
Browser
   |
   v
Notes Editor Razor View
   |
   v
notes-editor.js
   |
   +---- Add / Edit / Delete / Move blocks
   |
   +---- Update document state
   |
   +---- Render HTML / Preview
   |
   v
Browser localStorage
   |
   v
Restore saved document on reload
```

### Automatic Saving

C12 uses the Web Storage API.

The document is stored in browser localStorage.

This means:

- Data persists after reloading the page.
- Data normally persists after closing and reopening the browser.
- Data belongs to the browser profile and website origin.
- Data does not require a server request.
- Data is not shared automatically with another browser or device.

The user does not need to press a separate Save button after every modification.

### Document Restoration

When the editor opens, JavaScript reads previously stored document data.

If valid saved content exists, the editor restores the blocks and their order.

If no saved content exists, the editor initializes a new document state.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/NotesEditorController.cs` | Opens the editor page |
| `Frontend/Views/NotesEditor/Index.cshtml` | Razor View and editor interface |
| `Frontend/wwwroot/js/notes-editor.js` | Dynamic blocks, editing, reordering, and persistence |
| `Frontend/wwwroot/css/notes-editor.css` | Editor styling |
| `Frontend/Views/Shared/_Layout.cshtml` | Notes Editor navigation link |

### Testing in Visual Studio

C12 was verified manually using Visual Studio and Google Chrome.

Verification steps:

1. Build the updated CatalogManager solution.
2. Start the application using IIS Express.
3. Open `/NotesEditor`.
4. Add a Heading block.
5. Add a Paragraph block.
6. Add a Bulleted List block.
7. Add a Numbered List block.
8. Edit the text of several blocks.
9. Move blocks to change their order.
10. Delete a block.
11. Reload the page.
12. Confirm that the remaining text and block order are restored.
13. Close and reopen the browser and check the saved document again.

The user confirmed that the C12 editor works successfully.

### Inspecting localStorage

Open Google Chrome Developer Tools:

**F12 → Application → Local Storage → current website origin**

The stored document data can be inspected there.

This demonstrates that the browser, rather than the PostgreSQL database, is responsible for C12 document persistence.

### Database Changes

C12 does not require new PostgreSQL tables.

No Entity Framework Core migration is needed.

The existing CatalogManager database schema and catalog CRUD operations remain unchanged.

### Limitations

- Documents are stored only in the current browser profile.
- Documents are not automatically synchronized across devices.
- Clearing site storage may delete saved documents.
- Private browsing storage may be temporary.
- The editor requires JavaScript.
- localStorage has limited capacity.
- C12 does not yet provide real-time collaboration.
- Browser file import/export is planned for a later stage.

### Preparation for Future Stages

The C12 document and block model provides the foundation for additional coursework features.

**C13 — Cross-Device Synchronization**

Move the shared document state to the server and synchronize updates between browsers.

**C14 — Collaborative Change Visualization**

Highlight and animate document changes received from another client.

**C15 — Browser File API**

Allow users to import and export documents using browser file-handling APIs.

These features are planned and are not part of the completed C12 implementation.

### C12 Conclusion

C12 extends CatalogManager with a dynamic browser-based editor.

The stage demonstrates:

- DOM manipulation.
- Dynamic JavaScript collections.
- User interaction.
- Editable document blocks.
- Automatic persistence with localStorage.
- Restoration of browser state.

**C12 result: implemented and manually verified in the browser.**

---

## Postman Collection — QA1 + B1 + B9

Collection file:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

The collection includes:

- QA1 REST API automation.
- B1 pagination tests.
- B9 output caching tests.

### Import and Run

1. Start CatalogManager.
2. Import the collection into Postman.
3. Set the `baseUrl` collection variable.
4. Select the relevant folder.
5. Run using Collection Runner.
6. Review Passed, Failed, and Errors.

IIS Express URL:

`https://localhost:44361`

Docker URL:

`http://localhost:8085`

Use a development or test database when running data-changing tests.

---

## Local Setup Without Docker

### Restore Dependencies

```powershell
dotnet restore CatalogManager.slnx
```

### Configure PostgreSQL

Create the database:

`CatalogManagerDb`

Configure User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with the local PostgreSQL password.

### Apply Migrations

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

### Run the Application

Open the solution in Visual Studio and select the IIS Express launch profile.

Press:

**Ctrl + F5 — Start Without Debugging**

Alternatively:

```powershell
dotnet run --project Frontend
```

The URL depends on the launch profile.

### Application Pages

| Feature | Route |
|---|---|
| Products | `/Products` |
| Create Product | `/Products/Create` |
| Stores Map | `/StoresMap` |
| Statistics | `/Statistics` |
| Currency Rates | `/CurrencyRates` |
| Notes Editor | `/NotesEditor` |

Currency Rates requires access to the external PrivatBank API.

Notes Editor works with localStorage and does not require a new database table.

---

## Automated Testing

The solution includes three test projects.

| Project | Purpose |
|---|---|
| `UnitTests` | Unit tests with xUnit and Moq |
| `IntegrationTests` | HTTP integration tests using WebApplicationFactory and PostgreSQL |
| `E2ETests` | Browser-based Playwright tests |

### Integration Test Database

The integration tests use:

`CatalogManagerTestDb`

Configure the connection using User Secrets:

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

### Run QA2 with IIS Express

Keep CatalogManager running:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Run QA2 with Docker

Keep the Docker environment running:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "http://localhost:8085"
dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

### Run QA2 on GitHub

Open:

**GitHub → Actions → QA2 - Playwright E2E Tests**

The workflow builds and launches Docker, prepares PostgreSQL, installs Chromium, runs the tests, and uploads the results.

### Last Confirmed Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests before B2 | 65 |
| Additional B2 Service Tests | 4 |
| Playwright E2E Tests | 7 |
| **Total verified cases across separate runs** | **284** |

Important:

- These results were verified across separate test executions.
- They do not represent one single completed run of the entire updated solution.
- C12 was verified manually in the browser.
- No new automated C12 test count is claimed.
- A complete regression run after C12 is recommended before the final coursework defense.

---

## Code Coverage

Collect C# code coverage for the appropriate test projects:

```powershell
dotnet test UnitTests --collect:"XPlat Code Coverage"
dotnet test IntegrationTests --collect:"XPlat Code Coverage"
```

The Playwright project requires a running application and is executed separately.

Install ReportGenerator if necessary:

```powershell
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate a combined report:

```powershell
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open:

`CoverageReport/index.html`

An earlier pre-C5 coverage report showed:

- Line coverage: 76.1%.
- Branch coverage: 77.7%.
- Application layer line coverage: 100%.

These historical results are not claimed as current C12 code coverage.

---

## Security Notes

- Never commit PostgreSQL passwords or API tokens.
- Keep `.env` excluded from Git.
- Use User Secrets for development connections.
- Use GitHub Secrets for sensitive automation configuration.
- Check Postman exports for credentials.
- Run data-changing tests against development or test databases.
- MVC forms use antiforgery protection.
- REST API inputs use validation and business rules.
- Docker PostgreSQL is bound to localhost.
- robots.txt is not an access control mechanism.
- Exchange rates are indicative.
- localStorage is not a secure location for passwords or sensitive data.
- Avoid placing confidential information in the C12 notes editor.
- Server-side synchronization introduced in future stages will require additional security and data validation.
- Production deployment requires additional hardening.

---

## Project Status

### Implemented and Verified

- **C1 — MVC Application:** completed; video defense recorded.
- **C5 — REST API:** completed.
- **QA1 — Automated API Testing:** 62/62 and 124/124 assertions passed.
- **B1 — Pagination:** completed.
- **C8 — Interactive Stores Map:** completed.
- **B9 — Output Caching:** 48/48 Postman assertions passed.
- **C4 — Docker:** completed.
- **C6 — Bootstrap Landing Pages:** completed.
- **C7 — SEO:** Lighthouse SEO score 100/100.
- **F1 — Data Visualization:** completed.
- **QA2 — E2E Testing:** seven Playwright tests with successful GitHub Actions automation.
- **F2 — AJAX Autocomplete:** completed; six new integration tests passed.
- **B2 — PrivatBank Integration:** completed; four service tests passed.
- **C12 — Dynamic Notes Editor:** completed and manually verified.

### Latest Testing Summary

- 208 unit tests in the latest confirmed unit test run.
- 65 integration tests confirmed before B2.
- Four additional B2 service tests passed.
- Seven QA2 browser tests passed in GitHub Actions.
- 284 test cases accounted for across these separate runs.
- C12 browser functionality verified manually.

### Coursework Points

| Coursework Group | Points |
|---|---:|
| Implemented stages before C12 | 32 |
| C12 — Dynamic Notes Editor | 3 |
| **Total potential points** | **35** |

These points are for completed implementations and are not an officially awarded course grade.

### Remaining Tasks

- Verify GitHub Actions after the latest changes.
- Confirm that no local `.env` files are tracked by Git.
- Perform a complete regression check before final submission.
- Independently verify B9 cache HIT using diagnostics.
- Implement C13, C14, and C15.
- Prepare a combined defense after completing the next four-stage group.

### Planned Next Stages

**C13 — Cross-Device Synchronization (2 points)**

Extend the notes editor with server-side persistence and synchronization across browser sessions, potentially using SignalR.

**C14 — Collaborative Change Visualization (3 points)**

Show and visually highlight edits made in another browser.

**C15 — Browser File API (3 points)**

Implement import and export of documents using browser file-handling APIs.

### Upcoming Defense

The next combined defense will cover:

| Stage | Description | Points |
|---|---|---:|
| C12 | Dynamic Notes Editor | 3 |
| C13 | Cross-Device Synchronization | 2 |
| C14 | Collaborative Change Visualization | 3 |
| C15 | Browser File API | 3 |
| **Total** | | **11** |

After successfully implementing all four stages, the potential total will reach **43 points**.

---

CatalogManager is developed incrementally using Git commits, completion tags, automated testing, Docker, and GitHub Actions.
