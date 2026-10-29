# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests and a repeatable Postman API test suite.

The project was developed for the **Web Technologies** coursework:

- **C1 — MVC Application:** CRUD for three related entities with an optional product image URL.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation, and HTTP status codes.
- **QA1 — Automated Web API Testing:** repeatable Postman collection with positive and negative scenarios, dynamic IDs, assertions, and cleanup.
- **B1 — Pagination API:** offset pagination with `skip`, `limit`, and `nextLink` for collection endpoints.
- **C8 — External API Integration:** interactive stores map powered by OpenStreetMap and Leaflet.
- **B9 — API Output Caching:** in-memory response caching using ASP.NET Core Output Cache, cache expiration, and tag-based invalidation.

---

## Features

- **Categories:** create, read, update, and delete; duplicate names are rejected.
- **Stores:** manage name, address, latitude, and longitude.
- **Products:** manage name, description, price, optional image URL, category, and store.
- **Validation:** required values, price limits, coordinates, and valid related entity IDs.
- **Deletion rules:** categories and stores cannot be deleted while referenced by products.
- **MVC interface:** Razor Views, Bootstrap, form ViewModels, and antiforgery protection.
- **REST API:** JSON endpoints for Categories, Stores, and Products.
- **Pagination:** offset-based pagination for API collection endpoints.
- **Stores map:** interactive map, store markers, popups, name search, and Refresh without a full page reload.
- **API caching:** 30-second in-memory Output Cache for GET endpoints, with tag-based invalidation after successful API modifications.
- **Testing:** xUnit, Moq, HTTP integration tests, and an automated Postman collection.

---

## Technology Stack

- C# / .NET 10
- ASP.NET Core MVC, Web API, and Razor Views
- Bootstrap, HTML, CSS, and JavaScript
- Entity Framework Core 10
- PostgreSQL and Npgsql
- ASP.NET Core Output Caching (in-memory)
- xUnit and Moq
- ASP.NET Core `WebApplicationFactory`
- Postman Collection Runner with JavaScript assertions
- OpenStreetMap and Leaflet.js
- Coverlet and ReportGenerator
- Git and GitHub

---

## Solution Structure

```text
CatalogManager/
├── CatalogManager.slnx
├── Frontend/
│   ├── Controllers/           # MVC controllers
│   ├── ApiControllers/        # JSON API controllers
│   ├── Models/                # MVC form ViewModels
│   ├── DTOs/                  # API DTOs and pagination
│   ├── Views/
│   │   └── StoresMap/         # Interactive map page
│   ├── wwwroot/
│   │   ├── js/                # Map JavaScript
│   │   └── css/               # Map styles
│   ├── Program.cs             # Application and Output Cache setup
│   └── appsettings.Development.json
├── Application/               # Services and interfaces
├── Domain/                    # Domain entities
├── Infrastructure/            # EF Core, repositories, migrations
├── UnitTests/                 # Unit tests
├── IntegrationTests/          # HTTP integration tests
├── Postman/
│   └── CatalogManager_QA1_B1_B9.postman_collection.json
├── B1_PAGINATION_GUIDE.md
└── README.md
```

The Postman collection filename above is the recommended name for the final exported collection. The exported file must be placed in the `Postman` directory.

---

## Architecture and Data Model

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

HTTP client → API Controller → Application Service → Repository → EF Core / PostgreSQL

The MVC application and REST API use the same business services.

Each **Product** belongs to one **Category** and one **Store**.

A category or store can contain multiple products. Foreign-key constraints prevent deleting referenced categories and stores.

MVC uses dedicated form ViewModels:

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

API endpoints use separate create, update, and response DTOs to avoid directly exposing database entities.

---

## MVC Application (C1)

MVC controllers `CategoriesController`, `StoresController`, and `ProductsController` provide Create, Read, Update, and Delete operations through Razor Views in `Frontend/Views/`.

Input validation uses Data Annotations and service-layer business rules.

Form POST actions use antiforgery validation.

---

## REST API (C5)

| Entity | GET all / POST create | GET one / PUT update / DELETE |
|---|---|---|
| Categories | `/api/categories` | `/api/categories/{id}` |
| Stores | `/api/stores` | `/api/stores/{id}` |
| Products | `/api/products` | `/api/products/{id}` |

### HTTP Status Codes

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

---

## Automated Postman API Testing (QA1)

The project includes an automated Postman API test collection.

The original QA1 implementation covers 50 requests in four folders:

1. **Categories (13 requests):** listing, retrieving, creating, and updating categories; duplicate-name validation and invalid JSON.
2. **Stores (13 requests):** listing, retrieving, creating, and updating stores; required fields and coordinate validation.
3. **Products (15 requests):** listing, retrieving, creating, and updating products; price, name, foreign-key, and JSON validation.
4. **Cleanup and Relationship Checks (9 requests):** deletion conflicts, cleanup in dependency order, and subsequent `404` verification.

### How Automation Works

- `Scripts → Pre-request` and `Scripts → Post-response` prepare test data and verify responses using `pm.test` and `pm.expect`.
- Collection variables such as `baseUrl`, `runToken`, `categoryId`, `storeId`, and `productId` connect requests.
- IDs returned by POST requests are stored using `pm.collectionVariables.set(...)`.
- A run-specific token distinguishes newly created test data from previous runs.
- Cleanup requests delete products before their stores and categories.
- Negative test cases pass when the API returns the expected `400`, `404`, or `409` status.

### Verified QA1 Results

| Run | Iterations | Passed Assertions | Failed | Errors |
|---|---:|---:|---:|---:|
| Initial automated run | 1 | 62 | 0 | 0 |
| Repeatability verification | 2 | 124 | 0 | 0 |

Verified using Postman Collection Runner on **28 October 2026**.

The successful two-iteration run demonstrated that the workflow can be repeated without manually changing resource IDs.

---

## B1 — Pagination API

### Overview

B1 implements offset-based pagination for the REST API collection endpoints.

Pagination allows clients to retrieve a limited number of records rather than requesting the entire collection at once.

### Supported Endpoints

```http
GET /api/categories?skip=0&limit=10
GET /api/stores?skip=0&limit=10
GET /api/products?skip=0&limit=10
```

### Query Parameters

- `skip` — number of records to skip.
- `limit` — maximum number of records to return.
- `nextLink` — link to the next page when additional records are available.

When pagination is requested, the response includes the selected items, pagination parameters, total record count, and next-page link.

### Example

```http
GET /api/products?skip=0&limit=2
```

Example response structure:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Example Product"
    }
  ],
  "skip": 0,
  "limit": 2,
  "totalCount": 1,
  "nextLink": null
}
```

The example illustrates the pagination response structure; the actual product DTO contains additional fields.

### Compatibility

Existing collection endpoints remain compatible with earlier C5 clients.

Requests without pagination parameters preserve the original JSON array response.

### Documentation

Detailed implementation information, API examples, validation scenarios, and defense instructions are available in:

[B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md)

---

## C8 — Interactive Stores Map (OpenStreetMap + Leaflet)

### Overview

C8 integrates an external mapping service into CatalogManager using **OpenStreetMap** and **Leaflet.js**.

The application provides an interactive map at:

`/StoresMap`

Store locations are retrieved from the PostgreSQL database through the existing REST API:

```http
GET /api/stores
```

### Implemented Features

- **Interactive Map:** display an interactive map powered by OpenStreetMap and Leaflet.
- **Store Markers:** visualize stores using latitude and longitude.
- **Store Popups:** display store information and links to details.
- **Client-Side Search:** filter markers by store name.
- **Dynamic Counter:** show the number of visible stores.
- **Refresh Functionality:** reload store data without refreshing the entire page.
- **Invalid Coordinates Handling:** skip stores with invalid or missing coordinates.
- **Navigation:** open Stores Map from the application menu.

### Architecture

```text
Browser
   |
   v
StoresMap MVC Page
   |
   v
stores-map.js
   |
   v
GET /api/stores
   |
   v
StoresApiController
   |
   v
Application Service
   |
   v
Repository
   |
   v
PostgreSQL
```

Leaflet.js renders the map and manages markers and popups.

OpenStreetMap supplies geographic map tiles.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | MVC controller for the map page |
| `Frontend/Views/StoresMap/Index.cshtml` | Map interface and controls |
| `Frontend/wwwroot/js/stores-map.js` | API requests, markers, filtering, refresh |
| `Frontend/wwwroot/css/stores-map.css` | Map styling |
| `Frontend/Views/Shared/_Layout.cshtml` | Navigation link |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests |

### Manual Verification

The following functionality was manually verified:

- Map rendering.
- Store markers.
- Marker popups.
- Store name search.
- Search reset.
- Refresh functionality.
- Store coordinates returned through Postman.

### Integration Tests

C8 introduced `StoresMapIntegrationTests.cs`.

The tests verify the MVC page, map interface, and accessibility of related static resources.

Three integration test cases were added.

No external API key or additional EF Core migration is required.

Internet access is needed to load OpenStreetMap tiles and externally hosted Leaflet resources.

---

## B9 — API Output Caching

### Overview

B9 implements server-side response caching for the existing REST API using **ASP.NET Core Output Cache**.

The implementation uses the built-in in-memory cache rather than an external Redis server.

Caching is intended to reduce unnecessary response generation and repeated database access for identical GET requests.

### Cache Configuration

Output caching is configured in:

`Frontend/Program.cs`

The application registers Output Cache services using:

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("CategoriesCache", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .Tag("categories"));

    options.AddPolicy("StoresCache", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .Tag("stores"));

    options.AddPolicy("ProductsCache", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .Tag("products"));
});
```

The Output Cache middleware is enabled using:

```csharp
app.UseOutputCache();
```

### Cache Policies

| Policy | Resource | Expiration | Tag |
|---|---|---|---|
| `CategoriesCache` | Categories API | 30 seconds | `categories` |
| `StoresCache` | Stores API | 30 seconds | `stores` |
| `ProductsCache` | Products API | 30 seconds | `products` |

The corresponding policies are applied to GET actions in the API controllers using Output Cache attributes.

### Cached Endpoints

```http
GET /api/categories
GET /api/stores
GET /api/products
```

Collection requests with pagination parameters are also supported:

```http
GET /api/categories?skip=0&limit=10
GET /api/stores?skip=0&limit=10
GET /api/products?skip=0&limit=10
```

Different query-string parameters are taken into account when constructing the default Output Cache keys.

As a result, paginated requests can be cached separately from requests without pagination.

### Cache Expiration

Cached responses have a configured expiration time of **30 seconds**.

After the expiration period, the next eligible request must generate a fresh response.

The newly generated response can then be cached again.

### Cache Invalidation

The API controllers use `IOutputCacheStore` to invalidate cached responses after successful data modifications.

Cache entries are invalidated using:

```csharp
await cache.EvictByTagAsync(
    "categories",
    cancellationToken);
```

Equivalent invalidation is configured for stores and products.

### Invalidation Rules

| API Operation | Expected Cache Behavior |
|---|---|
| GET | Eligible responses are cached |
| Repeated GET | Cached response may be reused |
| POST | Relevant cache entries are invalidated after successful creation |
| PUT | Relevant cache entries are invalidated after successful update |
| DELETE | Relevant cache entries are invalidated after successful deletion |
| Cache expiration | Expired entries are regenerated on subsequent requests |

Cache invalidation is implemented for successful REST API modifications. Changes made directly in PostgreSQL or through MVC actions are not automatically covered by these API invalidation calls; such changes may remain invisible in cached API responses until expiration.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Program.cs` | Register Output Cache policies and middleware |
| `Frontend/ApiControllers/CategoriesApiController.cs` | Category caching and invalidation |
| `Frontend/ApiControllers/StoresApiController.cs` | Store caching and invalidation |
| `Frontend/ApiControllers/ProductsApiController.cs` | Product caching and invalidation |
| `Frontend/appsettings.Development.json` | Optional diagnostic logging configuration |

### Compatibility With Previous Stages

The B9 implementation preserves:

- Existing C5 CRUD endpoints.
- HTTP status codes and validation.
- B1 pagination behavior.
- QA1 automated API testing.
- C8 map functionality and store data loading.

The existing C# tests continue to pass after adding caching.

### B9 Postman Testing

The updated Postman collection contains a dedicated folder:

`B9 - API Caching`

It includes 24 HTTP requests with automated assertions.

The test scenarios cover:

- Creating temporary categories, stores, and products.
- Warming collection GET endpoints.
- Repeating GET requests.
- Updating data using PUT.
- Checking that updated data appears in GET responses.
- Checking paginated GET responses after updates.
- Deleting temporary entities.
- Checking that deleted entities no longer appear.
- Cleaning up temporary test records.

The workflow uses collection variables and dynamic resource IDs.

### Verified B9 Postman Results

| Metric | Result |
|---|---:|
| Requests | 24 |
| Assertions | 48 |
| Passed | 48 |
| Failed | 0 |
| Errors | 0 |
| Iterations | 1 |

**Result: 48 passed, 0 failed, 0 errors.**

Verified using Postman Collection Runner on **29 October 2026**.

These assertions confirm correct API responses after modifying and deleting records, as well as compatibility with B1 pagination.

The HTTP assertions alone do not prove that repeated responses were served from memory. Confirming an actual cache hit requires additional server-side diagnostics or instrumentation.

### Cache Diagnostics

For development diagnostics, the following log category can be configured in `Frontend/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.OutputCaching": "Debug"
    }
  }
}
```

This is an example of the Logging section, not necessarily a replacement for the full configuration file.

Output Cache diagnostics can be inspected while sending repeated GET requests in Postman.

The final Cache HIT verification is tracked separately from the completed response-correctness tests.

### Limitations

- The current implementation uses memory-local caching, not Redis.
- Cache entries are not shared across multiple application instances.
- Cache entries are cleared when the application process restarts.
- API modifications trigger invalidation, but direct database updates do not.
- MVC modifications are not covered by the API-controller invalidation logic.
- The cache is configured for a short 30-second lifetime.
- Cached HTTP responses do not necessarily include an `Age` or `Cache-Control` header.

No new PostgreSQL migrations are required for B9.

---

## Postman Collection — QA1 + B1 + B9

The recommended final collection is:

`CatalogManager API Tests - QA1 B1 B9`

Recommended export path:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

This collection combines the automated API tests from previous stages with pagination checks and API caching scenarios.

### How to Import and Run

1. Start CatalogManager in Visual Studio.
2. Import the JSON collection into Postman.
3. Open the collection's **Variables** tab.
4. Check the `baseUrl` value.
5. Ensure the URL matches the running HTTPS application.
6. Run the relevant folder with **1 iteration**.
7. Review Passed, Failed, Errors, and individual request results.

A previously verified IIS Express address was:

`https://localhost:44361`

The actual port may differ between computers or launch profiles.

### Safety Notes

The collection includes requests that create, modify, and delete data.

Run it only against a local development database or a dedicated test environment.

Do not execute data-changing Postman tests against production or valuable data.

---

## Requirements

- .NET 10 SDK
- Running PostgreSQL server
- Visual Studio with ASP.NET/web development tools, or a compatible editor
- Postman
- Internet access for OpenStreetMap and Leaflet
- Optional: pgAdmin
- Optional: ReportGenerator for coverage reports

No Redis installation or additional NuGet package is required for the current B9 implementation.

---

## Local Setup

### 1. Clone and Restore

```bash
git clone <YOUR_REPOSITORY_URL>
cd CatalogManager
dotnet restore CatalogManager.slnx
```

Replace `<YOUR_REPOSITORY_URL>` with the actual Git repository URL.

### 2. Configure the Development Database

Create a PostgreSQL database named:

`CatalogManagerDb`

Configure the database connection using **User Secrets**:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with your PostgreSQL password.

User Secrets are intended for local development and should not be used as a production secret store.

### 3. Apply Migrations

Install the EF Core CLI tool if necessary:

```bash
dotnet tool install --global dotnet-ef
```

Apply migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project Frontend
```

### 4. Start the Application

```bash
dotnet run --project Frontend
```

Open the local application URL displayed in the terminal.

The IIS Express address used by Visual Studio may differ from the URL shown by `dotnet run`.

### 5. Verify the API

Example endpoints:

```http
GET /api/categories
GET /api/stores
GET /api/products
```

Pagination example:

```http
GET /api/products?skip=0&limit=10
```

Interactive stores map:

`/StoresMap`

---

## C# Automated Testing

The solution contains two testing projects.

| Project | Purpose |
|---|---|
| `UnitTests` | Business services, MVC controllers, and API controllers tested using Moq |
| `IntegrationTests` | HTTP workflows through WebApplicationFactory and a separate PostgreSQL database |

### Test Database Setup

Create a separate database named:

`CatalogManagerTestDb`

Configure test credentials:

```bash
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

The integration test factory checks the test database name and applies migrations.

### Run All Tests

From the solution root:

```bash
dotnet test CatalogManager.slnx
```

Alternatively, in Visual Studio:

**Test → Test Explorer → Run All Tests**

### Last Confirmed C# Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 55 |
| **Total** | **263** |

**Result: 263 passed, 0 failed, 0 skipped.**

Verified in Visual Studio Test Explorer on **29 October 2026**, after applying the B9 controller changes.

These results include the previous functionality and C8 integration tests.

They do not represent 263 cache-specific tests.

---

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

Open:

`CoverageReport/index.html`

Exclude stale coverage files when merging results.

### Previous Coverage Results

The previously reviewed **pre-C5 report** showed:

- Line coverage: 76.1%.
- Branch coverage: 77.7%.
- Application layer line coverage: 100%.

These figures are historical and must not be treated as current B9 coverage.

---

## Security Notes

- Do not commit passwords, tokens, `secrets.json`, or other credentials.
- Store local PostgreSQL credentials using User Secrets.
- Check exported Postman collections for secrets before publishing.
- MVC POST forms include antiforgery protection.
- API requests are validated using DTOs and application services.
- Do not run data-changing Postman tests against production databases.
- In-memory Output Cache is intended for the current coursework implementation; additional cache and deployment considerations apply to distributed production environments.
- Additional security and deployment hardening are required before public production use.

---

## Project Status

### Implemented and Verified

- **C1 — MVC Application:** implemented; video defense recorded.
- **C5 — REST API:** implemented, manually tested, and committed to GitHub.
- **QA1 — Automated Web API Testing:** implemented; 62/62 assertions passed in one iteration and 124/124 in two iterations.
- **B1 — Pagination API:** implemented and verified.
- **C8 — Interactive Stores Map:** implemented and manually verified.
- **B9 — API Output Caching:** implemented; 263/263 existing C# tests and 48/48 B9 Postman assertions passed.

### Remaining B9 Verification

- Confirm actual Cache HIT through server-side diagnostics.
- Finalize the Postman collection export.
- Create the B9 Git commit and completion tag.

### Planned Next Stages

- **C4 — Docker Containerization:** containerize the ASP.NET Core application and PostgreSQL using Docker Compose.
- **C3 — CI/CD:** configure automated build and deployment.
- Prepare final video defenses for completed stages.

The project will continue to be developed incrementally, with each coursework stage committed and tagged separately in Git.
