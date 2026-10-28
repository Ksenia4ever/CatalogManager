
# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests and a repeatable Postman API test suite.

The project was developed for the **Web Technologies** coursework:

- **C1 — MVC Application:** CRUD for three related entities with an optional product image URL.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation and HTTP status codes.
- **QA1 — Automated Web API Testing:** repeatable Postman collection with positive and negative scenarios, dynamic IDs, assertions and cleanup.
- **B1 — Pagination API:** offset pagination with `skip`, `limit`, and `nextLink` for collection endpoints.
- **C8 — External API Integration:** interactive stores map powered by OpenStreetMap and Leaflet.

## Features

- **Categories:** create, read, update and delete; duplicate names are rejected.
- **Stores:** manage name, address, latitude and longitude.
- **Products:** manage name, description, price, optional image URL, category and store.
- **Validation:** required values, price limits, coordinates and valid related entity IDs.
- **Deletion rules:** categories and stores cannot be deleted while referenced by products.
- **MVC interface:** Razor Views, Bootstrap, form ViewModels and antiforgery protection.
- **REST API:** JSON endpoints for Categories, Stores and Products.
- **Pagination:** offset-based pagination for API collection endpoints.
- **Stores map:** interactive map, store markers, popups, name search, and Refresh without a full page reload.
- **Testing:** xUnit/Moq, HTTP integration tests and an automated Postman collection.

## Technology Stack

- C# / .NET 10; ASP.NET Core MVC, Web API and Razor Views; Bootstrap
- Entity Framework Core 10; PostgreSQL and Npgsql
- xUnit; Moq; ASP.NET Core `WebApplicationFactory`
- Postman Collection Runner with JavaScript post-response assertions
- OpenStreetMap tile service; Leaflet.js
- HTML, CSS and JavaScript
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
│   │   └── StoresMap/        # Interactive map page
│   ├── wwwroot/
│   │   ├── js/               # Map JavaScript
│   │   └── css/              # Map styles
│   └── Program.cs
├── Application/               # Services and interfaces
├── Domain/                    # Category, Store and Product entities
├── Infrastructure/            # DbContext, repositories, migrations
├── UnitTests/                 # Unit tests
├── IntegrationTests/          # HTTP + PostgreSQL integration tests
├── Postman/
│   └── CatalogManager_QA1_Automated.postman_collection.json
├── B1_PAGINATION_GUIDE.md
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

## B1 — Pagination API

### Overview

B1 implements offset-based pagination for the existing REST API collection endpoints.

Pagination allows API clients to request a limited number of records rather than retrieving the entire collection at once.

### Supported Endpoints

- `GET /api/categories`
- `GET /api/stores`
- `GET /api/products`

### Query Parameters

- `skip` — number of records to skip.
- `limit` — maximum number of records to return.
- `nextLink` — link to the next page when additional records are available.

### Example

```http
GET /api/products?skip=0&limit=10
```

The API returns the requested page of products and provides navigation information for retrieving the next page.

### Documentation

Detailed implementation information, API examples, validation scenarios, and defense instructions are provided in:

[B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md)

## C8 — Interactive Stores Map (OpenStreetMap + Leaflet)

### Overview

C8 integrates an external mapping service into CatalogManager using **OpenStreetMap** and **Leaflet.js**.

The application provides an interactive map at `/StoresMap`, displaying store locations based on geographic coordinates stored in the PostgreSQL database.

Store information is loaded dynamically through the existing REST API endpoint:

```http
GET /api/stores
```

Unlike hard-coded maps, the displayed markers use actual store data retrieved from the application.

### Implemented Features

- **Interactive Map:** display an interactive map powered by OpenStreetMap and Leaflet.
- **Store Markers:** visualize stores using their latitude and longitude coordinates.
- **Store Popups:** display store information and a link to store details when clicking a marker.
- **Client-Side Search:** filter markers by entering part of a store name.
- **Dynamic Counter:** show the number of currently displayed stores.
- **Refresh Functionality:** reload store information from the REST API without reloading the entire page.
- **Invalid Coordinates Handling:** safely skip stores with missing or invalid coordinates.
- **Navigation:** access the Stores Map page through the application menu.

### Architecture

The map functionality follows the existing layered architecture.

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

OpenStreetMap provides the geographic map tiles.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | MVC controller for the map page |
| `Frontend/Views/StoresMap/Index.cshtml` | Map interface, search field and Refresh button |
| `Frontend/wwwroot/js/stores-map.js` | Fetching API data, markers, popups, filtering and refresh |
| `Frontend/wwwroot/css/stores-map.css` | Map styling |
| `Frontend/Views/Shared/_Layout.cshtml` | Navigation link to Stores Map |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests for the map page and static resources |

### How It Works

1. The user opens `/StoresMap`.
2. Leaflet initializes the interactive map.
3. JavaScript sends an HTTP request to `GET /api/stores`.
4. The REST API retrieves store data from PostgreSQL.
5. The browser receives JSON containing store information and coordinates.
6. Stores with valid geographic coordinates are displayed as map markers.
7. Clicking a marker opens a popup with store details.
8. The search input filters the visible markers.
9. The Refresh button requests the latest data from the REST API.

This implementation reuses the existing Stores REST API without requiring an additional database schema.

### External Service Integration

**OpenStreetMap** supplies the map tiles, while **Leaflet.js** is responsible for interactive map rendering and user interactions.

The application does not require an external API key.

No additional Entity Framework Core migrations are necessary.

An internet connection is required to load OpenStreetMap tiles and externally hosted Leaflet resources.

### Manual Verification

1. Run the `Frontend` application.
2. Open `/StoresMap` or select **Stores Map** from the navigation menu.
3. Verify that the OpenStreetMap map loads successfully.
4. Check that store markers appear at their saved coordinates.
5. Click a marker and verify that its popup displays store information.
6. Type part of a store name into the search field.
7. Verify that only matching markers remain visible and the counter updates.
8. Clear the search field and confirm that all matching stores return.
9. Click **Refresh** and confirm that data reloads without refreshing the browser page.
10. Open Postman and send `GET /api/stores`.
11. Verify the `200 OK` response and the returned JSON data, including latitude and longitude.

### Integration Tests

C8 introduces `StoresMapIntegrationTests.cs`.

The integration tests verify:

- The `/StoresMap` MVC endpoint returns HTTP `200 OK`.
- The HTML page contains the map container and search interface.
- The page references the necessary JavaScript resources.
- The map JavaScript file is accessible.
- The map CSS file is accessible.

The test class contains **three test cases**: one MVC page test and two parameterized static resource tests.

### Verified Results

The following functionality was manually verified:

- Map rendering.
- Store markers.
- Marker popups.
- Store name search.
- Search reset.
- Refresh functionality.
- Store coordinates returned through Postman.

Visual Studio Test Explorer confirmed:

- **Total C# tests:** 263
- **Passed:** 263
- **Failed:** 0
- **Skipped:** 0

The successful test run includes the C8 integration tests and the previously implemented functionality.

## Requirements

- .NET 10 SDK
- Running PostgreSQL server
- Visual Studio with ASP.NET/web development tools or a compatible editor
- Postman for importing and running the automated QA1 collection
- Internet access for OpenStreetMap and Leaflet resources
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

Alternatively, use **Test → Test Explorer → Run All Tests** in Visual Studio.

### Last Confirmed C# Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 55 |
| **Total** | **263** |

**Result: 263 passed, 0 failed, 0 skipped.**

Verified in Visual Studio Test Explorer on **29 October 2026**, including the C8 map integration tests.

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

These numbers are not claimed as current C5, QA1, B1, or C8 coverage.

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
- **QA1 — Automated Web API Testing:** implemented; 62/62 assertions passed in one iteration and 124/124 in two iterations.
- **B1 — Pagination API:** implemented and verified.
- **C8 — Interactive Stores Map:** implemented and manually verified; all 263 C# tests passed successfully.

Future development may include containerization, caching, CI/CD, and other coursework stages.
