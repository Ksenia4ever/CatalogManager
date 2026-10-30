
# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests and a repeatable Postman API test suite.

The project was developed for the **Web Technologies** coursework:

- **C1 — MVC Application:** CRUD operations for three related entities with an optional product image URL.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation, and HTTP status codes.
- **QA1 — Automated Web API Testing:** Postman collection with positive and negative test cases, dynamic IDs, assertions, and cleanup.
- **B1 — Pagination API:** offset-based pagination using `skip`, `limit`, and `nextLink`.
- **C8 — External API Integration:** interactive stores map powered by OpenStreetMap and Leaflet.
- **B9 — API Output Caching:** in-memory response caching with expiration and tag-based invalidation.
- **C4 — Docker Containerization:** containerized ASP.NET Core application and PostgreSQL using Docker Compose.
- **C6 — Bootstrap Landing Page:** responsive product catalog and individual product landing pages.
- **C7 — SEO Optimization:** product-specific SEO metadata, canonical URLs, Open Graph tags, robots.txt, and Lighthouse auditing.
- **F1 — Data Visualization:** interactive statistical dashboard using Chart.js and data retrieved from PostgreSQL.

---

## Features

- **Categories:** create, read, update, and delete categories; duplicate names are rejected.
- **Stores:** manage names, addresses, and geographic coordinates.
- **Products:** manage names, descriptions, prices, optional image URLs, categories, and stores.
- **Validation:** required fields, product prices, coordinates, and related entity IDs.
- **Deletion rules:** categories and stores cannot be deleted while referenced by products.
- **MVC interface:** Razor Views, Bootstrap, ViewModels, and antiforgery validation.
- **REST API:** JSON endpoints for Categories, Stores, and Products.
- **Pagination:** offset-based pagination for API collection endpoints.
- **Stores Map:** interactive OpenStreetMap map with markers, popups, name search, and Refresh.
- **API Caching:** 30-second in-memory Output Cache with invalidation after successful API modifications.
- **Containerization:** Docker Compose with ASP.NET Core, PostgreSQL, and persistent storage.
- **Product Landing Pages:** responsive Bootstrap product cards and individual product presentation pages.
- **SEO Optimization:** metadata, canonical URLs, social sharing tags, robots.txt, and Lighthouse auditing.
- **Statistics Dashboard:** summary indicators, three interactive charts, and asynchronous data refreshing.
- **Testing:** xUnit, Moq, HTTP integration tests, and automated Postman collections.

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
- ASP.NET Core `WebApplicationFactory`
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

The local `.env` file contains environment-specific Docker configuration and must not be committed to Git.

---

## Architecture and Data Model

CatalogManager follows a layered architecture.

**MVC requests:**

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

**API requests:**

HTTP Client → API Controller → Application Service → Repository → EF Core / PostgreSQL

The MVC application and REST API share application services and business rules.

Each Product belongs to one Category and one Store. Categories and stores may have multiple products.

Foreign-key constraints prevent deleting categories and stores that are still referenced by products.

MVC uses dedicated form ViewModels:

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

REST API endpoints use separate request and response DTOs.

The statistics dashboard reads existing catalog data and transforms it into aggregated information for visual presentation.

---

## MVC Application (C1)

MVC controllers `CategoriesController`, `StoresController`, and `ProductsController` implement CRUD operations through Razor Views.

Input validation uses Data Annotations and application service business rules.

Form POST actions use antiforgery validation.

The MVC application is also the foundation for the product landing pages introduced in C6 and optimized in C7.

---

## REST API (C5)

| Entity | Collection endpoint | Individual resource endpoint |
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

## Automated Postman API Testing (QA1)

The original QA1 implementation contains 50 requests in four folders:

1. **Categories (13):** CRUD operations, duplicate names, invalid JSON, and validation.
2. **Stores (13):** CRUD operations, required fields, and coordinate validation.
3. **Products (15):** CRUD operations, price validation, foreign keys, and invalid JSON.
4. **Cleanup and Relationship Checks (9):** deletion conflicts, cleanup, and missing-resource checks.

### Automation

- Pre-request and post-response scripts use JavaScript.
- Assertions use `pm.test()` and `pm.expect()`.
- Collection variables store dynamically generated IDs.
- Run-specific tokens distinguish test data.
- Cleanup follows foreign-key dependencies.
- Negative scenarios pass when the expected HTTP error status is returned.

### Verified QA1 Results

| Iterations | Passed Assertions | Failed | Errors |
|---|---:|---:|---:|
| 1 | 62 | 0 | 0 |
| 2 | 124 | 0 | 0 |

Verified on **28 October 2026**.

---

## B1 — Pagination API

### Overview

B1 implements offset-based pagination for REST API collection endpoints.

### Supported Requests

```http
GET /api/categories?skip=0&limit=10
GET /api/stores?skip=0&limit=10
GET /api/products?skip=0&limit=10
```

### Query Parameters

- `skip` — number of records to skip.
- `limit` — maximum number of records to return.
- `nextLink` — URL of the next page when additional results are available.

Paginated responses contain:

- `items`
- `skip`
- `limit`
- `totalCount`
- `nextLink`

Requests without pagination parameters preserve the original JSON array response, maintaining compatibility with C5.

Pagination is implemented at the database query level through Entity Framework Core.

Detailed documentation:

[B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md)

---

## C8 — Interactive Stores Map

### Overview

C8 integrates OpenStreetMap and Leaflet.js to display store locations on an interactive map.

The page is available at:

`/StoresMap`

Store data is retrieved through:

```http
GET /api/stores
```

### Implemented Features

- Interactive map using OpenStreetMap tiles.
- Store markers based on saved coordinates.
- Popups containing store information.
- Search by store name.
- Dynamic visible marker counter.
- Refresh without reloading the page.
- Validation of geographic coordinates.
- Navigation through the application menu.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | MVC controller |
| `Frontend/Views/StoresMap/Index.cshtml` | Map page |
| `Frontend/wwwroot/js/stores-map.js` | API loading and map interactions |
| `Frontend/wwwroot/css/stores-map.css` | Map styling |
| `Frontend/Views/Shared/_Layout.cshtml` | Navigation |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests |

### Verification

Manual verification covered map rendering, markers, popups, search, reset, refresh, and store coordinates.

Three integration test cases verify the MVC page and related static resources.

No additional database migrations or external API key were required.

---

## B9 — API Output Caching

### Overview

B9 implements server-side response caching using ASP.NET Core Output Cache.

The application uses in-memory caching without Redis.

### Cache Policies

| Policy | Resource | Duration | Tag |
|---|---|---|---|
| `CategoriesCache` | Categories API | 30 seconds | `categories` |
| `StoresCache` | Stores API | 30 seconds | `stores` |
| `ProductsCache` | Products API | 30 seconds | `products` |

Output Cache services are registered using `AddOutputCache()`.

The middleware is enabled using `UseOutputCache()`.

GET actions use the corresponding cache policies.

### Cache Behavior

- Eligible GET responses can be served from memory.
- Responses expire after 30 seconds.
- Query parameters are considered when generating cache keys.
- B1 pagination is compatible with Output Cache.
- Successful API POST, PUT, and DELETE operations invalidate relevant tags using `IOutputCacheStore.EvictByTagAsync()`.

Direct database modifications and MVC changes are not automatically covered by API-controller invalidation calls. Cached responses may remain unchanged until expiration.

### B9 Testing

The Postman collection contains a folder named `B9 - API Caching`.

It contains 24 requests testing:

- Temporary test-data creation.
- Repeated GET requests.
- Record updates.
- Responses after cache invalidation.
- Compatibility with pagination.
- Record deletion.
- Test-data cleanup.

### Verified Results

| Metric | Result |
|---|---:|
| Requests | 24 |
| Assertions | 48 |
| Passed | 48 |
| Failed | 0 |
| Errors | 0 |

Verified on **29 October 2026**.

These HTTP assertions verify response correctness and data visibility after API modifications. They do not independently establish an actual Cache HIT; that requires server-side diagnostics.

### Limitations

- Memory cache belongs to a single application instance.
- Cache data is not shared between multiple containers.
- Cache data is lost when the application process restarts.
- Cached responses have a 30-second lifetime.
- API writes invalidate relevant cache tags.
- Direct PostgreSQL and MVC changes are not covered by API-controller invalidation.

No additional database migrations were required.

---

## C4 — Docker Containerization

### Overview

C4 containerizes CatalogManager using Docker and Docker Compose.

The application runs in a Linux container using ASP.NET Core 10, while PostgreSQL 17 runs in a separate container.

Docker Compose manages networking, environment configuration, application startup, and persistent storage.

### Docker Architecture

```text
Windows Host
    |
    | http://localhost:8085
    |
    v
Docker Compose
    |
    +--- web container
    |      ASP.NET Core 10
    |      Port 8080
    |      |
    |      | Internal network: db:5432
    |      v
    +--- db container
           PostgreSQL 17
           Port 5432
           |
           v
       Docker Volume
       catalogmanager_pgdata
```

The web application is accessible at:

`http://localhost:8085`

PostgreSQL is accessible from Windows at:

`127.0.0.1:55432`

Within Docker Compose, the application connects to the PostgreSQL service using `db:5432`.

### Docker Files

| File | Purpose |
|---|---|
| `Dockerfile` | Multi-stage ASP.NET Core build |
| `.dockerignore` | Build-context exclusions |
| `compose.yaml` | Web and PostgreSQL services |
| `.env.example` | Example configuration |
| `.env` | Local configuration, excluded from Git |
| `.gitignore` | Exclusions for secrets and generated files |

### Dockerfile

The multi-stage Dockerfile uses:

- The .NET 10 SDK image for restoring, building, and publishing.
- The ASP.NET Core 10 runtime image for running the published application.

The final container starts with:

`dotnet CatalogManager.dll`

This avoids including the complete SDK in the runtime image.

### Docker Compose Services

**web**

- Builds the application using the root Dockerfile.
- Runs ASP.NET Core on internal port `8080`.
- Publishes host port `8085`.
- Connects to PostgreSQL through the internal network.
- Receives the connection string through environment variables.
- Waits for the database health check.

**db**

- Uses the official `postgres:17` image.
- Runs PostgreSQL on internal port `5432`.
- Publishes host port `55432` on the loopback interface.
- Uses `pg_isready` for health checking.
- Stores data in a named Docker volume.

### Persistent Storage

The PostgreSQL container uses:

`catalogmanager_pgdata`

The volume stores data independently of the container lifecycle.

During verification, a category named `DockerTestCategory` remained available after restarting both containers.

### Environment Configuration

Docker Compose reads configuration from `.env`.

The repository contains `.env.example` as a template.

Actual database passwords and other secrets must not be committed.

### Requirements

- Docker Desktop
- Docker Compose
- Linux container support
- Internet connection for downloading images
- Sufficient disk space

### Build and Start

Open Windows PowerShell in the solution root.

Check Docker:

```powershell
docker version
```

Validate Compose:

```powershell
docker compose config --quiet
```

Build and start:

```powershell
docker compose up --build -d
```

Check containers:

```powershell
docker compose ps -a
```

The database should become `Healthy`, and the web container should be running.

### Database Migrations

A new PostgreSQL database inside Docker initially contains no application tables.

Apply the existing Entity Framework Core migrations.

For the local Windows workflow, install the CLI tool if needed:

```powershell
dotnet tool install --global dotnet-ef --version "10.0.*"
```

Stop IIS Express before the migration workflow to avoid locked assemblies.

Temporarily configure the Docker database connection:

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

Do not accidentally target the original local PostgreSQL database on port `5432`.

### Verify the Application

Open:

`http://localhost:8085`

Check:

`http://localhost:8085/api/categories`

A fresh empty database returns:

```json
[]
```

Other API endpoints:

```http
GET http://localhost:8085/api/stores
GET http://localhost:8085/api/products
```

The interactive map:

`http://localhost:8085/StoresMap`

### Docker Commands

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

View database logs:

```powershell
docker compose logs db --tail=80
```

Do not use `docker compose down -v` unless the database volume should intentionally be deleted.

### Verified C4 Results

Verified on **29 October 2026**:

- Docker CLI and Docker Engine worked.
- Compose configuration validation passed.
- The .NET 10 application image built successfully.
- PostgreSQL 17 was downloaded and started.
- Docker networking and named volume were created.
- PostgreSQL reached the `Healthy` state.
- ASP.NET Core started successfully.
- Existing EF Core migrations were applied.
- MVC and REST API requests worked.
- A category was created and persisted.
- Data survived container restart.

### Limitations

- The configuration targets local development.
- The application currently uses local HTTP.
- The database port is bound to localhost.
- Migrations are applied manually.
- Additional TLS, secret management, backups, and security hardening would be required for production hosting.

---

## C6 — Bootstrap Product Landing Pages

### Overview

C6 improves product presentation through responsive Bootstrap landing pages.

The original MVC project already supported CRUD operations, but product information was displayed using conventional data-oriented views.

C6 introduces visually improved product cards and individual product presentation pages.

Existing models, controllers, business services, and PostgreSQL tables are reused.

### Product Catalog

The product catalog is available at:

`/Products`

Products are displayed in a responsive Bootstrap card layout.

Each card provides a product overview and navigation to its individual landing page.

### Individual Product Landing Page

Each product has a dedicated page:

`/Products/Details/{id}`

Example:

`/Products/Details/1`

### Implemented Features

- Responsive Bootstrap product cards.
- Individual product landing pages.
- Product name and description.
- Product price.
- Optional product image.
- Category and store information.
- Navigation between catalog and details pages.
- Custom CSS styling.
- Responsive desktop and mobile layouts.
- Reuse of existing MVC architecture.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Views/Products/Index.cshtml` | Product catalog with Bootstrap cards |
| `Frontend/Views/Products/Details.cshtml` | Product landing page |
| `Frontend/wwwroot/css/product-landing.css` | Product presentation styling |
| `Frontend/Controllers/ProductsController.cs` | Existing MVC controller |
| `Domain/Entities/Product.cs` | Existing product data |

### Image Handling

Images use the existing optional `ImageUrl` property.

The database stores image URLs rather than binary image data.

The interface can display a placeholder when a product has no image.

### Verification

Manual checks covered:

- Product catalog rendering.
- Navigation to product landing pages.
- Product text and prices.
- Category and store information.
- Image handling.
- Responsive Bootstrap styling.
- Existing MVC functionality.

All existing automated tests continued to pass.

No additional database migrations or REST API endpoints were necessary.

---

## C7 — SEO Optimization

### Overview

C7 improves the technical search engine optimization of the product landing pages developed in C6.

The goal is to make individual product pages more understandable to search engines by providing relevant metadata and appropriate indexing instructions.

SEO metadata is generated for the selected product, so different products can have different titles and descriptions.

The implementation uses the existing ASP.NET Core MVC and Razor infrastructure.

No additional database tables, migrations, or external cloud services are required.

### Implemented SEO Features

- Product-specific HTML page titles.
- Meta descriptions based on product information.
- Canonical URLs for individual product pages.
- Open Graph metadata for social sharing.
- Reuse of existing Bootstrap product landing pages.
- A `robots.txt` file with crawler instructions.
- SEO verification using Google Chrome Lighthouse.

### SEO Page Titles

Individual product pages use descriptive titles based on product information.

For example, the Lenovo IdeaPad 5 product page has a title containing the product name and CatalogManager.

A descriptive title helps search engines and users identify the purpose of the page.

### Meta Descriptions

Product pages include SEO descriptions derived from the corresponding product information.

Meta descriptions provide a concise summary of page content.

Search engines may use them when generating search result snippets, although the final snippet is determined by the search engine.

### Canonical URLs

Canonical URLs identify the preferred URL for a page.

This helps indicate the intended page address when the same or similar content may be accessible through multiple URLs.

For product landing pages, canonical metadata refers to the corresponding product details address.

### Open Graph Metadata

Open Graph tags provide structured metadata for services that support rich link previews.

Depending on the available product information, these tags can describe the page title, description, URL, and image.

They improve how product links can be presented when shared through compatible services.

### Robots.txt

The crawler instruction file is located at:

`Frontend/wwwroot/robots.txt`

It is publicly accessible at:

`/robots.txt`

Its contents are:

```text
User-agent: *
Allow: /
```

`User-agent: *` applies the rules to all crawlers.

`Allow: /` permits crawling across the site according to these rules.

This does not force search engines to index the site and does not grant public Internet access to a localhost application.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Views/Shared/_Layout.cshtml` | Rendering shared SEO metadata |
| `Frontend/Views/Products/Details.cshtml` | Product-specific SEO information |
| `Frontend/wwwroot/robots.txt` | Search crawler instructions |
| `Frontend/wwwroot/css/product-landing.css` | Existing C6 styling |

### SEO Audit Using Google Lighthouse

The SEO audit was performed using Google Chrome DevTools Lighthouse.

Audited page:

`https://localhost:44361/Products/Details/1`

Audit configuration:

- **Mode:** Navigation
- **Device:** Desktop
- **Category:** SEO

### Verified Lighthouse Results

| Metric | Result |
|---|---:|
| Lighthouse SEO Score | **100/100** |
| Passed Audits | **9** |
| Not Applicable | **1** |
| Failed Applicable SEO Audits | **0** |

**Audit date:** 30 October 2026.

The audited page was the Lenovo IdeaPad 5 product landing page.

Lighthouse also indicated an additional manual review item related to structured data validation. This was not reported as an automatic SEO failure.

The result confirms that the audited page passed all applicable automated Lighthouse SEO checks.

It does not guarantee search engine indexing or ranking.

### Functional and Regression Testing

The existing C# test suite passed after C7:

- 208 unit tests.
- 55 integration tests.
- **263 tests passed in total.**

The `/robots.txt` endpoint was successfully opened in Chrome.

No additional database migrations were required.

### Limitations

- Lighthouse was run against a local application.
- The score applies to the audited page and selected configuration.
- Search engine indexing cannot be demonstrated from localhost.
- Lighthouse covers basic technical SEO checks, not every ranking factor.
- Public deployment would require additional hosting and security configuration.

---

## F1 — Data Visualization

### Overview

F1 introduces a statistical visualization dashboard into CatalogManager using **Chart.js**.

The dashboard presents aggregated information about products, categories, and stores in an interactive format.

Instead of manually reviewing individual records, users can quickly understand the structure of the product catalog through summary indicators and charts.

The dashboard reuses existing application data stored in PostgreSQL.

No new database tables, migrations, or separate backend project are required.

### Statistics Page

The statistics dashboard is available at:

`/Statistics`

The page can be opened through the application's navigation menu.

It uses Bootstrap for page layout and Chart.js for rendering statistical diagrams.

### Statistics Data Endpoint

The frontend retrieves the statistics asynchronously through:

```http
GET /Statistics/Data
```

The endpoint returns JSON data derived from the existing catalog records.

JavaScript uses the Fetch API to retrieve this information and update the dashboard without navigating to another page.

### Summary Indicators

The dashboard displays summary information about the product catalog:

- Total number of products.
- Total number of categories.
- Total number of stores.

These indicators help users understand the current size of the catalog.

### Implemented Charts

**1. Products by Category**

Displays the number of products belonging to each category.

This visualization helps compare the distribution of products across categories.

**2. Products by Store**

Displays how many products are associated with each store.

This visualization helps identify stores with larger or smaller product assortments.

**3. Product Price Distribution**

Groups products into price ranges and displays the number of products in each range.

This visualization helps analyze the overall distribution of product prices.

The charts are generated from actual catalog data rather than hard-coded demonstration values.

### Interactive Features

- Automatic statistics loading when the page opens.
- Asynchronous retrieval of JSON data.
- Chart rendering using Chart.js.
- Dashboard summary indicators.
- Refresh functionality for updating displayed statistics.
- Loading and error handling.
- Responsive layout using Bootstrap and custom CSS.
- Navigation through the existing MVC application.

### Architecture

The data flow is:

Browser → Statistics Page → JavaScript Fetch API → StatisticsController → Application Data → PostgreSQL

The controller provides the statistics page and a separate JSON endpoint.

The JavaScript file processes the returned JSON and renders the diagrams.

This approach separates data retrieval from presentation logic.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StatisticsController.cs` | Statistics page and JSON data endpoint |
| `Frontend/Views/Statistics/Index.cshtml` | Dashboard Razor View |
| `Frontend/wwwroot/js/statistics.js` | Fetch requests and Chart.js initialization |
| `Frontend/wwwroot/css/statistics.css` | Dashboard styling and responsiveness |
| `Frontend/Views/Shared/_Layout.cshtml` | Statistics navigation link |
| `IntegrationTests/Tests/StatisticsIntegrationTests.cs` | HTTP integration tests for F1 |

### Chart.js Integration

Chart.js is a JavaScript charting library used to visualize data.

The application uses it to render interactive diagrams on HTML canvas elements.

The chart data is generated dynamically from the JSON response.

This means that changes in the underlying product catalog can be reflected in the dashboard after updating the statistics.

### Testing

Four new integration test cases were added for the F1 functionality.

The tests use the existing ASP.NET Core integration testing infrastructure based on `WebApplicationFactory`.

The complete regression test suite was executed after implementing F1.

### Verified F1 Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 59 |
| **Total** | **267** |
| Failed | **0** |
| Skipped | **0** |

**Result: 267 passed, 0 failed, 0 skipped.**

Verified in Visual Studio on **30 October 2026**.

Compared with the previous confirmed result of 263 tests, the F1 implementation adds four integration test cases.

The statistics dashboard was also reported as working correctly in the running application.

### Limitations

- The dashboard displays statistics derived from the current catalog data.
- Data is refreshed through HTTP requests rather than a real-time push connection.
- Chart.js requires the corresponding JavaScript resources to load successfully.
- The statistics page is intended for catalog visualization rather than advanced business intelligence.
- No new database schema or migration is required.

### F1 Conclusion

F1 successfully extends CatalogManager with an interactive statistical dashboard.

It provides product distribution visualizations, summary indicators, and asynchronous data updates.

The implementation preserves the functionality of the previous coursework stages.

**All 267 automated C# tests passed successfully.**

---

## Postman Collection — QA1 + B1 + B9

The combined collection is stored in the repository at:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

It combines automated QA1, pagination, and API caching scenarios.

### Import and Run

1. Start CatalogManager through Visual Studio or Docker.
2. Import the collection into Postman.
3. Check the collection variable `baseUrl`.
4. Use the correct local API address.
5. Run the relevant folder through Collection Runner.
6. Review Passed, Failed, Errors, and individual request results.

The original IIS Express environment used:

`https://localhost:44361`

The Docker environment uses:

`http://localhost:8085`

Run data-changing Postman tests only against development or test environments.

---

## Local Setup Without Docker

### 1. Clone and Restore

```bash
git clone <YOUR_REPOSITORY_URL>
cd CatalogManager
dotnet restore CatalogManager.slnx
```

Replace `<YOUR_REPOSITORY_URL>` with the actual repository URL.

### 2. Configure PostgreSQL

Create the PostgreSQL database:

`CatalogManagerDb`

Configure credentials using User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

### 3. Apply Migrations

```bash
dotnet ef database update --project Infrastructure --startup-project Frontend
```

### 4. Start the Application

```bash
dotnet run --project Frontend
```

The local URL depends on the selected development profile.

---

## C# Automated Testing

The solution contains two testing projects.

| Project | Purpose |
|---|---|
| `UnitTests` | Business services, MVC controllers, API controllers, and related logic using Moq |
| `IntegrationTests` | HTTP workflows using WebApplicationFactory and a dedicated PostgreSQL test database |

### Integration Test Database

Integration tests use a separate database:

`CatalogManagerTestDb`

Configure its connection using User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

### Run Tests

```bash
dotnet test CatalogManager.slnx
```

Alternatively, use Visual Studio:

**Test → Test Explorer → Run All Tests**

### Last Confirmed Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 59 |
| **Total** | **267** |

**267 passed, 0 failed, 0 skipped.**

Verified on **30 October 2026**, after implementing F1.

---

## Code Coverage

Collect coverage:

```bash
dotnet test CatalogManager.slnx --collect:"XPlat Code Coverage"
```

Install ReportGenerator if necessary:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate the combined report:

```bash
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open:

`CoverageReport/index.html`

The previously reviewed pre-C5 coverage report showed:

- Line coverage: 76.1%.
- Branch coverage: 77.7%.
- Application layer line coverage: 100%.

These are historical results and are not claimed as current F1 coverage measurements.

---

## Security Notes

- Never commit PostgreSQL passwords, tokens, or secret connection strings.
- Keep local `.env` files excluded from Git.
- Use `.env.example` for configuration documentation.
- Use User Secrets for local development credentials.
- Check exported Postman collections for secrets.
- Run data-changing automated requests only against disposable databases.
- MVC forms use antiforgery protection.
- REST API inputs are validated with DTOs and service-layer rules.
- The Docker database port is bound to localhost.
- Rotate credentials that have been exposed publicly.
- `robots.txt` is not an access-control or security mechanism.
- Public production deployment requires additional security hardening.

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
- **C7 — SEO Optimization:** implemented; Lighthouse SEO 100/100.
- **F1 — Data Visualization:** implemented; three Chart.js diagrams, a statistics dashboard, and 267/267 C# tests passed.

### Remaining Verification and Repository Tasks

- B9: independently confirm actual Cache HIT using server-side diagnostics.
- Verify that local `.env` files are not tracked in Git.
- Rotate any credentials exposed in screenshots or logs.
- Finalize the F1 commit, README update, and completion tag.

### Planned Next Stages

- **QA2 — End-to-End Testing:** implement automated browser-based tests and evaluate coverage.
- **C9 — Analytics Integration:** optional deferred stage.

The project is developed incrementally, using separate commits and completion tags for coursework stages.
