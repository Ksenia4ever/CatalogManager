
# CatalogManager

CatalogManager is a coursework web application for managing products, categories, and stores. It combines an ASP.NET Core MVC interface, a REST API, PostgreSQL persistence, and additional modules for search, mapping, statistics, collaborative notes, and real-time updates.

The solution uses C# / .NET 10, ASP.NET Core, Entity Framework Core, Razor Views, JavaScript, and Docker Compose. It includes unit, integration, Postman API, and Playwright end-to-end tests.

## 1. Prerequisites

### Required Software

- .NET 10 SDK.
- Git.
- PostgreSQL 17 or Docker Desktop with Docker Compose.
- Chrome, Edge, or another modern browser.
- Internet access for externally hosted JavaScript libraries, OpenStreetMap, and PrivatBank API.

### Recommended Tools

- Visual Studio with ASP.NET and web development support and .NET 10 compatibility.
- Postman for REST API testing.
- Docker Desktop for containerized deployment.
- PowerShell for running development commands.

### Backend Dependencies

NuGet packages are restored automatically from the project files.

Main dependencies:

- ASP.NET Core MVC and Web API
- Entity Framework Core 10
- Npgsql PostgreSQL provider
- Lucene.NET 4.8 beta
- ASP.NET Core SignalR
- ASP.NET Core Output Caching
- xUnit and Moq
- Microsoft Playwright
- Coverlet

### Frontend Libraries

- Bootstrap
- jQuery
- Select2
- Leaflet
- Chart.js
- SignalR JavaScript Client
- Fetch API
- HTML Drag and Drop API
- Browser File API

Some frontend libraries are loaded from CDNs and require internet access.

## 2. Getting Started

Clone the repository:

```powershell
git clone https://github.com/Ksenia4ever/CatalogManager.git
cd CatalogManager
git switch develop
```

Restore NuGet dependencies:

```powershell
dotnet restore CatalogManager.slnx
```

Alternatively, extract the source code ZIP and open `CatalogManager.slnx` in Visual Studio.

## 3. Local Setup — Visual Studio and PostgreSQL

### Step 1. Configure PostgreSQL

Install PostgreSQL 17 and make sure its service is running.

Create a database named:

`CatalogManagerDb`

The default local PostgreSQL port is `5432`.

### Step 2. Configure the Connection String

Use .NET User Secrets to avoid storing credentials in the repository.

Run from the solution directory:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with your PostgreSQL password.

### Step 3. Install EF Core CLI

```powershell
dotnet tool install --global dotnet-ef --version "10.0.*"
```

If the tool is already installed, update it instead:

```powershell
dotnet tool update --global dotnet-ef --version "10.0.*"
```

### Step 4. Apply Migrations

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

This creates the database schema using the existing migrations.

### Step 5. Run the Application

Open `CatalogManager.slnx` in Visual Studio.

Select **IIS Express** and press **Ctrl + F5**.

Application URL:

`https://localhost:44361`

The IIS Express port may differ on another computer. Use the HTTPS address shown by Visual Studio when the application starts.

Alternatively, run:

```powershell
dotnet run --project Frontend --launch-profile https
```

This launch profile uses:

`https://localhost:7168`

For local HTTPS certificate setup:

```powershell
dotnet dev-certs https --trust
```

### Step 6. Prepare Sample Data

Open the Categories page and create a category.

Open the Stores page and create a store.

Then open Products and create a product.

Products must reference existing categories and stores.

For map functionality, stores should have valid latitude and longitude values.

## 4. Docker Deployment

Docker Compose runs the ASP.NET Core application and PostgreSQL in separate containers.

### Step 1. Configure Environment Variables

Copy the environment template:

```powershell
Copy-Item .env.example .env
notepad .env
```

Set your own password:

```dotenv
POSTGRES_PASSWORD=YOUR_STRONG_LOCAL_PASSWORD
```

Do not commit `.env` to Git.

### Step 2. Start Docker Containers

```powershell
docker compose up --build -d
```

Check their status:

```powershell
docker compose ps -a
```

### Step 3. Apply Database Migrations

The Docker configuration exposes PostgreSQL through localhost port `55432`.

From Windows PowerShell in the solution directory:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=55432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_STRONG_LOCAL_PASSWORD"

dotnet ef database update --project Infrastructure --startup-project Frontend

Remove-Item Env:ConnectionStrings__DefaultConnection
```

Replace the password with the value from your `.env` file.

### Step 4. Open the Application

Docker application URL:

`http://localhost:8085`

For example:

`http://localhost:8085/Products`

To inspect logs:

```powershell
docker compose logs web --tail=100
```

To stop containers without deleting the database volume:

```powershell
docker compose down
```

PostgreSQL data is stored in the `catalogmanager_pgdata` Docker volume.

Do not use `docker compose down -v` unless you intend to remove the database volume.

Shared notes are stored as JSON files inside the web application's directory. The supplied Docker Compose configuration does not provide a separate persistent volume for these files, so notes may be lost if the web container is recreated.

The C10/C11 event history is stored in memory and resets when the web application restarts.

## 5. Application Pages

| Feature | Route |
|---|---|
| Categories | `/Categories` |
| Stores | `/Stores` |
| Products | `/Products` |
| Product Creation | `/Products/Create` |
| Stores Map | `/StoresMap` |
| Statistics | `/Statistics` |
| Product Search | `/ProductSearch` |
| Currency Rates | `/CurrencyRates` |
| Notes Editor | `/NotesEditor?doc=shared` |
| Connectivity Comparison | `/Connectivity` |

### REST API

The main API endpoints are:

- `/api/categories`
- `/api/stores`
- `/api/products`
- `/api/search/products?q=example`

The API supports CRUD operations, validation, and pagination.

## 6. Completed Coursework Stages

| Stage | Description | Points |
|---|---|---:|
| C1 | MVC application with related entities and CRUD operations | 3 |
| C5 | REST API with JSON DTOs and HTTP validation | 3 |
| QA1 | Automated Web API testing with Postman | 2 |
| B1 | REST API pagination | 2 |
| C8 | Interactive stores map using Leaflet and OpenStreetMap | 3 |
| B9 | ASP.NET Core Output Caching and invalidation | 3 |
| C4 | Docker containerization | 1 |
| C6 | Bootstrap product landing pages | 3 |
| C7 | SEO optimization | 1 |
| F1 | Statistics dashboard using Chart.js | 2 |
| QA2 | Playwright end-to-end testing | 3 |
| F2 | AJAX autocomplete using Select2 | 4 |
| B2 | PrivatBank external API integration | 2 |
| C12 | Dynamic notes editor | 3 |
| C13 | Cross-browser notes synchronization | 2 |
| C14 | Collaborative change visualization | 3 |
| C15 | Browser File API import/export | 3 |
| B4 | Lucene.NET full-text product search | 3 |
| F3 | Drag-and-drop notes block sorting | 1 |
| C10 | WebSockets, Long Polling, and Adaptive Polling | 5 |
| C11 | SignalR real-time connectivity | 3 |
| **Total** | **21 implemented stages** | **55** |

These are potential coursework points, not an officially awarded grade.

### C1 — MVC Application

The project implements three related entities: Category, Store, and Product.

The application supports creating, reading, updating, and deleting records.

Implementation:

- `Domain/Entities/`
- `Frontend/Controllers/`
- `Frontend/Views/`
- `Application/`
- `Infrastructure/`

### C5 — REST API

Implemented JSON CRUD endpoints for categories, stores, and products.

The API uses DTOs, input validation, and appropriate HTTP status codes.

Implementation:

- `Frontend/ApiControllers/`
- `Frontend/DTOs/`

### QA1 — Automated API Testing

Created a Postman collection with automated positive and negative HTTP tests, response validation, and dynamic variables.

Implementation:

- `Postman/`

### B1 — Pagination API

Implemented server-side pagination using `skip`, `limit`, and `nextLink`.

The API preserves backward compatibility with requests without pagination parameters.

Implementation:

- `Frontend/ApiControllers/`
- `Infrastructure/Repositories/`
- `B1_PAGINATION_GUIDE.md`

### C8 — Stores Map

Implemented an interactive map using Leaflet and OpenStreetMap.

Stores are displayed as markers with searchable names and details.

Implementation:

- `Frontend/Controllers/StoresMapController.cs`
- `Frontend/Views/StoresMap/`
- `Frontend/wwwroot/js/stores-map.js`

### B9 — Output Caching

Implemented caching of REST API GET responses for categories, stores, and products.

Cached results expire after 30 seconds and can be invalidated after data modifications.

Implementation:

- `Frontend/Program.cs`
- `Frontend/ApiControllers/`

### C4 — Docker Containerization

The application can run using Docker Compose with separate ASP.NET Core and PostgreSQL services.

Implementation:

- `Dockerfile`
- `compose.yaml`
- `.env.example`

### C6 — Bootstrap Landing Pages

Implemented responsive product cards and individual product detail pages.

Implementation:

- `Frontend/Views/Products/`
- `Frontend/wwwroot/css/product-landing.css`

### C7 — SEO Optimization

Added product-specific SEO metadata, canonical URLs, Open Graph tags, and robots.txt.

Technical SEO was evaluated using Chrome Lighthouse.

Implementation:

- `Frontend/Views/Products/Details.cshtml`
- `Frontend/Views/Shared/_Layout.cshtml`
- `Frontend/wwwroot/robots.txt`

### F1 — Data Visualization

Implemented a statistics dashboard using Chart.js.

The dashboard displays aggregated catalog data.

Implementation:

- `Frontend/Controllers/StatisticsController.cs`
- `Frontend/Views/Statistics/`
- `Frontend/wwwroot/js/statistics.js`

### QA2 — End-to-End Testing

Implemented seven browser-based E2E tests using Playwright and Chromium.

GitHub Actions automatically runs the tests inside a Docker-based environment.

Implementation:

- `E2ETests/CatalogManagerBrowserTests.cs`
- `.github/workflows/qa2-playwright.yml`

### F2 — AJAX Autocomplete

Implemented category and store autocomplete fields using jQuery and Select2.

Search begins after entering three characters, with a short delay before sending AJAX requests.

Implementation:

- `Frontend/Controllers/ProductAutocompleteController.cs`
- `Frontend/wwwroot/js/product-autocomplete.js`
- `Frontend/Views/Products/Create.cshtml`
- `Frontend/Views/Products/Edit.cshtml`

### B2 — External API Integration

Integrated the PrivatBank public API to display currency exchange rates and an approximate currency converter.

Implementation:

- `Frontend/Services/PrivatBankCurrencyRatesService.cs`
- `Frontend/Controllers/CurrencyRatesController.cs`
- `Frontend/Views/CurrencyRates/`

### C12 — Dynamic Notes Editor

Implemented a block-based editor supporting headings, paragraphs, and lists.

The original local editor was extended in later stages with shared server-side persistence.

Implementation:

- `Frontend/Controllers/NotesEditorController.cs`
- `Frontend/Views/NotesEditor/`
- `Frontend/wwwroot/js/notes-editor.js`

### C13 — Cross-Browser Synchronization

Implemented synchronization of shared notes using SignalR.

Documents are stored as versioned JSON files on the server.

Implementation:

- `Frontend/HUBS/NotesHub.cs`
- `Frontend/Services/SharedNotesStore.cs`
- `Frontend/wwwroot/js/notes-editor.js`

### C14 — Collaborative Change Visualization

Added visual notifications for remote document changes, including added, updated, moved, and deleted blocks.

Implementation:

- `Frontend/wwwroot/js/notes-editor.js`
- `Frontend/wwwroot/css/notes-editor.css`

### C15 — Browser File API

Implemented JSON document import and JSON/Markdown export using FileReader, Blob, and browser object URLs.

Implementation:

- `Frontend/wwwroot/js/notes-editor.js`

### B4 — Full-Text Product Search

Implemented a product search API using Lucene.NET.

The search indexes product names and descriptions and returns relevance-ranked results.

Implementation:

- `Frontend/Services/ProductSearchService.cs`
- `Frontend/ApiControllers/ProductSearchApiController.cs`
- `Frontend/Controllers/ProductSearchController.cs`
- `Frontend/Views/ProductSearch/`

### F3 — Drag-and-Drop Sorting

Added drag-and-drop reordering to the notes editor.

The updated order is saved through the existing shared-document mechanism.

Implementation:

- `Frontend/wwwroot/js/notes-editor.js`

### C10 — Connectivity Comparison

Implemented three methods of client-server event delivery:

- WebSockets
- Long Polling
- Adaptive Polling

The module displays live product events, connection statistics, delivery delays, and a bounded server event history.

Implementation:

- `Frontend/Controllers/ConnectivityController.cs`
- `Frontend/Services/ConnectivityEventStore.cs`
- `Frontend/wwwroot/js/connectivity.js`
- `Frontend/Views/Connectivity/`

### C11 — SignalR Real-Time Updates

Extended C10 with real-time SignalR notifications.

A hosted service forwards catalog events to connected SignalR clients.

Implementation:

- `Frontend/HUBS/ConnectivityHub.cs`
- `Frontend/Services/ConnectivitySignalRForwarder.cs`
- `Frontend/wwwroot/js/c11-signalr.js`

## 7. Automated Testing

The solution contains:

- UnitTests
- IntegrationTests
- E2ETests

### Unit Tests

```powershell
dotnet test UnitTests/CatalogManager.UnitTests.csproj
```

### Integration Tests

Integration tests use a separate database named `CatalogManagerTestDb`.

Create this database before running the tests.

Configure the connection string:

```powershell
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

Run:

```powershell
dotnet test IntegrationTests/CatalogManager.IntegrationTests.csproj
```

The test factory checks the database name and applies EF Core migrations.

### Playwright E2E Tests

Build the E2E project:

```powershell
dotnet build E2ETests/CatalogManager.E2ETests.csproj
```

Install Chromium with Playwright using PowerShell 7:

```powershell
pwsh E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

Start the web application before executing local browser tests.

For IIS Express:

```powershell
$env:CATALOGMANAGER_E2E_BASE_URL = "https://localhost:44361"

dotnet test E2ETests/CatalogManager.E2ETests.csproj
```

For Docker, set the base URL to:

`http://localhost:8085`

E2E tests may modify development data.

### GitHub Actions

The QA2 workflow is stored in:

`.github/workflows/qa2-playwright.yml`

It automatically prepares Docker, PostgreSQL, database migrations, and Chromium before executing seven Playwright tests.

The latest observed GitHub Actions run after the navigation-test reliability fix completed successfully.

### Test Results

Previously recorded results:

| Test Category | Result |
|---|---|
| Unit tests | 208 passed |
| Integration tests | 69 passed |
| Playwright E2E | 7 passed in the latest observed GitHub Actions run |

Unit and integration test numbers refer to previously recorded runs. They should be verified again for the final checkout.

## 8. Code Coverage

Collect .NET code coverage:

```powershell
dotnet test UnitTests/CatalogManager.UnitTests.csproj --collect:"XPlat Code Coverage"

dotnet test IntegrationTests/CatalogManager.IntegrationTests.csproj --collect:"XPlat Code Coverage"
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

The previously generated coverage report describes an earlier project version and should not be treated as current coverage for all 21 stages.

The seven E2E scenarios cover navigation, category CRUD and validation, access to product pages, the stores map, statistics, and robots.txt.
