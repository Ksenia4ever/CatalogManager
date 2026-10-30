
# CatalogManager

CatalogManager is a layered **ASP.NET Core MVC + REST API** application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated C# tests and a repeatable Postman API test suite.

The project was developed for the **Web Technologies** coursework:

- **C1 — MVC Application:** CRUD operations for three related entities with an optional product image URL.
- **C5 — REST API:** JSON CRUD endpoints, DTOs, validation, and HTTP status codes.
- **QA1 — Automated Web API Testing:** Postman collection with positive and negative test cases, dynamic IDs, assertions, and cleanup.
- **B1 — Pagination API:** offset-based pagination using `skip`, `limit`, and `nextLink`.
- **C8 — External API Integration:** interactive stores map powered by OpenStreetMap and Leaflet.
- **B9 — API Output Caching:** in-memory response caching with expiration and tag-based invalidation.
- **C4 — Docker Containerization:** containerized ASP.NET Core application and PostgreSQL database using Docker Compose.
- **C6 — Bootstrap Landing Page:** responsive product catalog and individual product landing pages with images, descriptions, prices, and related information.

---

## Features

- **Categories:** create, read, update, and delete categories; duplicate names are rejected.
- **Stores:** manage store names, addresses, and geographic coordinates.
- **Products:** manage product names, descriptions, prices, optional image URLs, categories, and stores.
- **Validation:** required fields, product price limits, valid coordinates, and valid related entity IDs.
- **Deletion rules:** categories and stores cannot be deleted while referenced by products.
- **MVC interface:** Razor Views, Bootstrap, ViewModels, and antiforgery validation.
- **REST API:** JSON endpoints for Categories, Stores, and Products.
- **Pagination:** offset-based pagination for API collection endpoints.
- **Stores Map:** interactive map, store markers, popups, name search, and Refresh without a full page reload.
- **API Caching:** 30-second in-memory Output Cache with invalidation after successful REST API modifications.
- **Containerization:** Docker Compose configuration for ASP.NET Core and PostgreSQL, with persistent database storage.
- **Product Landing Pages:** responsive Bootstrap product cards and detailed product presentation pages.
- **Testing:** xUnit, Moq, HTTP integration tests, and an automated Postman collection.

---

## Technology Stack

- C# / .NET 10
- ASP.NET Core MVC, Web API, and Razor Views
- Bootstrap, HTML, CSS, and JavaScript
- Entity Framework Core 10
- PostgreSQL 17 and Npgsql
- ASP.NET Core Output Caching
- Docker and Docker Compose
- xUnit and Moq
- ASP.NET Core `WebApplicationFactory`
- Postman Collection Runner
- OpenStreetMap and Leaflet.js
- Coverlet and ReportGenerator
- Git and GitHub

---

## Solution Structure

```text
CatalogManager/
├── CatalogManager.slnx
├── Frontend/
│   ├── Controllers/
│   ├── ApiControllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Views/
│   │   ├── Products/
│   │   │   ├── Index.cshtml
│   │   │   └── Details.cshtml
│   │   ├── StoresMap/
│   │   └── Shared/
│   ├── wwwroot/
│   │   ├── js/
│   │   │   └── stores-map.js
│   │   └── css/
│   │       ├── stores-map.css
│   │       └── product-landing.css
│   └── Program.cs
├── Application/
├── Domain/
├── Infrastructure/
├── UnitTests/
├── IntegrationTests/
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

A local `.env` file is also used by Docker Compose. It contains environment-specific configuration and must not be committed to Git.

---

## Architecture and Data Model

CatalogManager follows a layered architecture.

**MVC requests:**

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

**API requests:**

HTTP Client → API Controller → Application Service → Repository → EF Core / PostgreSQL

The MVC application and REST API share the same business services.

Each Product belongs to one Category and one Store. A Category or Store can contain multiple Products.

Foreign-key constraints prevent deleting categories and stores that are still referenced by products.

MVC uses dedicated form ViewModels:

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

API endpoints use separate request and response DTOs.

---

## MVC Application (C1)

MVC controllers `CategoriesController`, `StoresController`, and `ProductsController` implement Create, Read, Update, and Delete operations through Razor Views.

Input validation uses Data Annotations and application service business rules.

Form POST actions use antiforgery validation.

The MVC interface is also the foundation for the product landing pages introduced in C6.

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
- Tests use `pm.test()` and `pm.expect()`.
- Collection variables store dynamically generated IDs.
- Test data is distinguished using run-specific tokens.
- Cleanup follows foreign-key dependencies.
- Negative scenarios pass when the expected error status is returned.

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
- `nextLink` — URL of the next page when more results are available.

The paginated response contains:

- `items`
- `skip`
- `limit`
- `totalCount`
- `nextLink`

Requests without pagination parameters preserve the original JSON array response, maintaining compatibility with C5.

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

- Interactive map with OpenStreetMap tiles.
- Store markers based on saved coordinates.
- Popups containing store information.
- Search by store name.
- Dynamic visible marker counter.
- Refresh without reloading the entire page.
- Handling of invalid coordinates.
- Navigation through the application menu.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Controllers/StoresMapController.cs` | MVC controller |
| `Frontend/Views/StoresMap/Index.cshtml` | Map page |
| `Frontend/wwwroot/js/stores-map.js` | API requests and map interactions |
| `Frontend/wwwroot/css/stores-map.css` | Map styling |
| `Frontend/Views/Shared/_Layout.cshtml` | Navigation |
| `IntegrationTests/Tests/StoresMapIntegrationTests.cs` | Integration tests |

### Verification

Manual verification covered map rendering, markers, popups, search, reset, refresh, and store coordinates.

Three C8 integration test cases verify the MVC page and related static resources.

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

Output Cache services are registered in `Frontend/Program.cs` using `AddOutputCache()`.

The middleware is enabled using `UseOutputCache()`.

GET actions use the corresponding Output Cache policies.

### Cache Behavior

- Eligible GET responses can be served from memory.
- Responses expire after 30 seconds.
- Different query parameters are accounted for in cache keys.
- B1 pagination is compatible with Output Cache.
- Successful POST, PUT, and DELETE operations invalidate the relevant cache entries through `IOutputCacheStore.EvictByTagAsync()`.

Direct database changes and MVC modifications are not automatically covered by API-controller invalidation calls. Cached responses may therefore remain unchanged until expiration.

### B9 Testing

The Postman collection contains a folder named `B9 - API Caching`.

It contains 24 requests testing:

- Temporary test-data creation.
- Repeated GET requests.
- Updating records.
- Responses after cache invalidation.
- Compatibility with pagination.
- Deleting records.
- Cleanup of temporary resources.

### Verified Results

| Metric | Result |
|---|---:|
| Requests | 24 |
| Assertions | 48 |
| Passed | 48 |
| Failed | 0 |
| Errors | 0 |

Verified on **29 October 2026**.

These HTTP tests confirm correct responses and data visibility after API modifications. They do not independently prove Cache HIT; confirmation of actual cache reuse requires server-side diagnostics.

### Limitations

- Memory cache is local to each application instance.
- Cache data is not shared between multiple containers.
- Cache data is lost when the application process restarts.
- Cached responses have a 30-second lifetime.
- API writes invalidate relevant cache tags.
- Direct PostgreSQL and MVC changes are not covered by API-controller invalidation.

No new database migrations were required.

---

## C4 — Docker Containerization

### Overview

C4 containerizes CatalogManager using Docker and Docker Compose.

The application runs in a Linux container using **ASP.NET Core 10**, while PostgreSQL 17 runs in a separate container.

Docker Compose manages the application, database, internal networking, environment configuration, and persistent storage.

This allows the application and its database to run without manually starting PostgreSQL or IIS Express on the host computer.

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

The web application is accessible from Windows at:

`http://localhost:8085`

The PostgreSQL container is accessible from Windows at:

`127.0.0.1:55432`

Within the Docker Compose network, the web application connects to PostgreSQL using the service name `db` and port `5432`.

### Docker Files

| File | Purpose |
|---|---|
| `Dockerfile` | Multi-stage build of the ASP.NET Core application |
| `.dockerignore` | Excludes unnecessary files from the build context |
| `compose.yaml` | Defines web and PostgreSQL services |
| `.env.example` | Example environment configuration |
| `.env` | Local environment configuration, excluded from Git |
| `.gitignore` | Prevents local secrets and generated files from being committed |

All Docker configuration files are located in the root of the solution.

### Dockerfile

The Dockerfile uses a multi-stage build.

**Build stage:**

- Uses the .NET 10 SDK image.
- Restores NuGet dependencies.
- Builds and publishes the ASP.NET Core application in Release mode.

**Runtime stage:**

- Uses the ASP.NET Core 10 runtime image.
- Copies the published application.
- Starts the application using `dotnet CatalogManager.dll`.

This avoids including the entire .NET SDK in the final runtime image.

### Docker Compose Services

**web**

- Builds the application using the root Dockerfile.
- Runs ASP.NET Core on internal port `8080`.
- Publishes the application on host port `8085`.
- Connects to the PostgreSQL container through the Docker network.
- Receives the connection string through environment variables.
- Waits for the database health check before startup.

**db**

- Uses the official `postgres:17` Docker image.
- Runs PostgreSQL on internal port `5432`.
- Publishes port `55432` to the local Windows loopback interface.
- Uses environment variables to configure the database.
- Uses a health check based on `pg_isready`.
- Stores database data in a named Docker volume.

### Persistent Storage

The PostgreSQL container uses the named volume:

`catalogmanager_pgdata`

This volume stores database data independently of the container filesystem.

During testing, a category named `DockerTestCategory` was created through the MVC interface.

After running:

```powershell
docker compose restart
```

the category remained available through the REST API.

This confirmed data persistence after restarting the containers.

### Environment Configuration

Docker Compose reads local configuration from `.env`.

The repository includes `.env.example` as a template.

To configure a new environment, create a local `.env` based on the example and supply a suitable PostgreSQL password.

The actual `.env` file must remain excluded from version control.

Passwords must not be placed directly into `compose.yaml`, the Dockerfile, or committed configuration files.

### Requirements for Docker

- Docker Desktop
- Docker Compose
- Linux container support
- Sufficient disk space for .NET and PostgreSQL images
- Internet connection for the initial image download

The development environment used Docker Desktop with the Linux container engine.

### Build and Start

Open Windows PowerShell in the root directory of CatalogManager.

Check Docker:

```powershell
docker version
```

Validate the Compose configuration:

```powershell
docker compose config --quiet
```

Build and start the containers:

```powershell
docker compose up --build -d
```

The `-d` option runs containers in detached mode.

### Check Container Status

```powershell
docker compose ps -a
```

Expected services:

- `catalogmanager-db-1`
- `catalogmanager-web-1`

The database container should reach the `Healthy` state.

The web container should reach the `Up` state.

### Database Migrations

The PostgreSQL database created by Docker Compose is initially empty.

Existing EF Core migrations must be applied to create the database schema.

The application uses the same migrations stored in the Infrastructure project.

For the local Windows migration workflow, install the EF Core CLI tool if necessary:

```powershell
dotnet tool install --global dotnet-ef --version "10.0.*"
```

Stop IIS Express before building or applying migrations to avoid locked assembly files.

In the same PowerShell session, configure a temporary connection string for the Docker database:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=55432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_DOCKER_PASSWORD"
```

Replace the placeholder with the password from the local `.env` file. Do not publish or commit the real password.

Apply the existing migrations:

```powershell
dotnet ef database update --project Infrastructure --startup-project Frontend
```

This command targets the Docker PostgreSQL database when the environment variable is set correctly.

After migration, remove the temporary override from the current PowerShell session:

```powershell
Remove-Item Env:ConnectionStrings__DefaultConnection
```

**Important:** do not run the migration command against the wrong database. The Docker PostgreSQL instance is exposed on host port `55432`, while the original local PostgreSQL instance may use port `5432`.

### Verify the Application

Open the application:

`http://localhost:8085`

Check the categories API:

`http://localhost:8085/api/categories`

A new empty database initially returns:

```json
[]
```

After creating a category using the MVC interface, the API returns the saved category as JSON.

The following endpoints are also available:

```http
GET http://localhost:8085/api/stores
GET http://localhost:8085/api/products
```

The interactive map is available at:

`http://localhost:8085/StoresMap`

### Restart Containers

```powershell
docker compose restart
```

The application and database containers restart without intentionally deleting their persistent storage.

### Stop Containers

```powershell
docker compose down
```

This stops and removes the Compose containers and network while preserving the named database volume by default.

To start the environment again:

```powershell
docker compose up -d
```

Do not use `docker compose down -v` unless intentionally deleting the named volume and its database data.

### View Logs

```powershell
docker compose logs web --tail=80
```

To view PostgreSQL logs:

```powershell
docker compose logs db --tail=80
```

### Verified C4 Results

The following operations were successfully completed on **29 October 2026**:

- Docker CLI and Docker Engine were verified.
- Docker Compose configuration validation passed.
- The multi-stage .NET 10 Docker image was built successfully.
- The official PostgreSQL 17 image was downloaded.
- Docker network and named volume were created.
- PostgreSQL reached the `Healthy` state.
- ASP.NET Core container reached the `Up` state.
- Existing EF Core migrations were applied successfully.
- `/api/categories` returned an empty JSON array from the fresh database.
- A new category was created through the MVC application.
- The category appeared in the REST API JSON response.
- Both containers were restarted.
- The saved category remained available after restart.
- Git ignore rules were checked to exclude `.env`.

### C4 Limitations

- Docker Compose currently targets local development and coursework demonstration.
- The application uses HTTP on the local host port `8085`.
- The database is exposed only on the local loopback interface.
- Docker does not automatically apply EF Core migrations in the current setup.
- The current configuration does not constitute a production-ready cloud deployment.
- Additional production security, backup, TLS, and deployment configuration would be necessary for public hosting.

---

## C6 — Bootstrap Product Landing Pages

### Overview

C6 introduces an improved, responsive user interface for presenting products.

The existing MVC application previously displayed product information using conventional CRUD-oriented pages.

The C6 implementation improves the presentation by introducing Bootstrap-based product cards and individual product landing pages.

Each product has a dedicated Details page displaying its information in a more visually appealing format.

The implementation reuses the existing Product entity, MVC controller, application services, and PostgreSQL database.

No additional database entities, migrations, or REST API endpoints are required.

### Product Catalog

The product listing page is available at:

`/Products`

Products are displayed as responsive Bootstrap cards.

Each card provides a compact overview of a product and navigation to its individual page.

The catalog layout adapts to different screen sizes using Bootstrap's responsive grid system.

### Individual Product Landing Page

Each product has its own URL:

`/Products/Details/{id}`

For example:

`/Products/Details/1`

The landing page presents the selected product using a dedicated visual layout rather than only a standard CRUD details table.

### Implemented Features

- Responsive Bootstrap product cards.
- Individual landing page for each product.
- Product name and description.
- Product price.
- Product image using the optional `ImageUrl` property.
- Category information.
- Store information.
- Navigation between the product catalog and individual product pages.
- Custom CSS styling for improved visual presentation.
- Layout adapted to desktop and smaller screens.
- Reuse of existing MVC functionality and business logic.

### Implementation Files

| File | Responsibility |
|---|---|
| `Frontend/Views/Products/Index.cshtml` | Responsive product catalog |
| `Frontend/Views/Products/Details.cshtml` | Individual product landing page |
| `Frontend/wwwroot/css/product-landing.css` | Custom landing page and catalog styling |
| `Frontend/Controllers/ProductsController.cs` | Existing MVC controller reused without requiring new actions |
| `Domain/Entities/Product.cs` | Existing product data, including optional image URL |

### Bootstrap Integration

Bootstrap is used for responsive layout, spacing, typography, cards, and navigation.

The custom `product-landing.css` file provides additional styling for the product presentation.

The implementation separates visual presentation from business logic.

Product information continues to be loaded through the existing MVC architecture:

Browser → ProductsController → ProductService → ProductRepository → PostgreSQL

### Image Handling

The existing `ImageUrl` property is used to display product images.

The images are referenced through URLs instead of storing binary image content in the database.

The product landing page can present products without requiring any changes to the database schema.

### Verification

The C6 implementation was manually verified in the running application.

Verification covered:

- Product catalog rendering.
- Navigation to individual product details.
- Product images and text information.
- Price presentation.
- Category and store information.
- Responsive Bootstrap layout.
- Existing MVC functionality.

The existing automated tests were also run successfully after implementing C6.

No changes to API contracts, EF Core migrations, Docker configuration, or B9 caching policies were required.

---

## Postman Collection — QA1 + B1 + B9

The combined collection is intended to be stored at:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

It combines automated QA1 tests with pagination and API caching scenarios.

### Import and Run

1. Start CatalogManager through Visual Studio or Docker.
2. Import the collection into Postman.
3. Check the collection variable `baseUrl`.
4. For IIS Express, use the applicable HTTPS localhost address.
5. For Docker, use `http://localhost:8085`.
6. Run the selected folder with one iteration.
7. Inspect Passed, Failed, Errors, and individual request results.

The original IIS Express environment used:

`https://localhost:44361`

The Docker environment uses:

`http://localhost:8085`

Postman tests that modify data must only be executed against development or test environments.

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

Create a local PostgreSQL database:

`CatalogManagerDb`

Configure the connection using User Secrets:

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

The development URL may differ from the Docker URL and from the IIS Express launch profile.

---

## C# Automated Testing

The solution contains two test projects.

| Project | Purpose |
|---|---|
| `UnitTests` | Unit tests for business services, MVC controllers, and API controllers using Moq |
| `IntegrationTests` | HTTP workflows using WebApplicationFactory and a dedicated PostgreSQL test database |

### Integration Test Database

The integration tests use a separate database named:

`CatalogManagerTestDb`

Configure its connection using User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

### Run Tests

```bash
dotnet test CatalogManager.slnx
```

Alternatively, open Visual Studio:

**Test → Test Explorer → Run All Tests**

### Last Confirmed Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 208 |
| Integration Tests | 55 |
| **Total** | **263** |

**263 passed, 0 failed, 0 skipped.**

Verified in Visual Studio on **29 October 2026**, after implementing B9.

All existing automated tests were also reported as passing after the C6 changes; the latest exact test count has not been independently updated here.

The Docker deployment itself was verified separately through container startup, EF Core migration execution, MVC operations, and REST API requests.

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

These historical figures are not claimed as current C6 coverage.

---

## Security Notes

- Never commit PostgreSQL passwords, tokens, or connection strings containing secrets.
- Keep `.env` excluded from Git.
- Use `.env.example` to document required environment variables without including real credentials.
- User Secrets are intended for local development.
- Verify Docker Compose configuration before starting containers.
- Check exported Postman collections for secrets.
- Run data-changing Postman requests only against disposable development or test databases.
- MVC forms use antiforgery protection.
- API data is validated using DTOs and service-layer rules.
- Docker database ports are bound to localhost in the current configuration.
- Replace any credentials exposed in screenshots or shared logs.
- Additional security hardening is required before production deployment.

---

## Project Status

### Implemented and Verified

- **C1 — MVC Application:** implemented; video defense recorded.
- **C5 — REST API:** implemented and tested.
- **QA1 — Automated Web API Testing:** implemented; 62/62 and 124/124 assertions passed.
- **B1 — Pagination API:** implemented and verified.
- **C8 — Interactive Stores Map:** implemented and verified.
- **B9 — API Output Caching:** implemented; existing C# tests and 48/48 B9 Postman assertions passed.
- **C4 — Docker Containerization:** successfully built and launched with PostgreSQL 17; migrations, MVC operations, REST API, and restart persistence verified.
- **C6 — Bootstrap Landing Page:** implemented and verified; responsive product catalog and individual product presentation pages.

### Remaining Verification and Repository Tasks

- B9: independently confirm actual Cache HIT using server-side diagnostics.
- Verify that local `.env` files are not tracked in Git.
- Rotate any credentials previously exposed in screenshots or logs.
- Finalize the C6 commit, README update, and completion tag.

### Planned Next Stages

- **C7 — SEO Optimization:** improve product landing page metadata and perform an SEO audit.
- **F1 — Data Visualization:** add charts and statistical visualizations.
- **QA2 — End-to-End Testing:** implement automated browser-based tests and evaluate coverage.
- **C9 — Analytics Integration:** optional deferred stage.

The project is developed incrementally, with separate commits and completion tags for coursework stages.
