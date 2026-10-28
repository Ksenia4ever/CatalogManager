# CatalogManager

CatalogManager is a layered ASP.NET Core MVC and REST API application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes automated unit and integration tests.

The project was developed as part of the **Web Technologies** coursework and includes:

- **C1 — MVC Application:** CRUD operations for three related entities.
- **C5 — REST API:** JSON-based CRUD operations with HTTP status codes and Postman testing.

## Features

- **Categories:** create, view, edit, and delete categories.
- **Stores:** manage store names, addresses, and geographic coordinates.
- **Products:** manage product names, descriptions, prices, and optional image URLs; assign each product to a category and a store.
- **Validation:** reject invalid product prices, empty required fields, invalid coordinates, and references to nonexistent categories or stores.
- **Category uniqueness:** prevent duplicate category names and return appropriate errors.
- **Deletion rules:** prevent deleting a category or store while products reference it.
- **MVC interface:** Razor Views with Bootstrap styling, dedicated form ViewModels, and antiforgery validation.
- **REST API:** JSON endpoints for Categories, Stores, and Products with GET, POST, PUT, and DELETE operations.
- **Automated testing:** unit and HTTP integration tests for MVC and REST API functionality.

## Technology Stack

- C# / .NET 10
- ASP.NET Core MVC and ASP.NET Core Web API
- Razor Views and Bootstrap
- Entity Framework Core 10
- PostgreSQL with Npgsql
- xUnit and Moq
- ASP.NET Core WebApplicationFactory
- Postman
- Coverlet and ReportGenerator
- Git and GitHub

## Solution Structure

```text
CatalogManager/
├── CatalogManager.slnx
├── Frontend/
│   ├── Controllers/        # MVC controllers
│   ├── ApiControllers/     # REST API controllers
│   ├── Models/             # MVC ViewModels
│   ├── DTOs/
│   │   ├── Categories/
│   │   ├── Stores/
│   │   └── Products/
│   ├── Views/              # Razor Views
│   ├── wwwroot/            # Static resources
│   └── Program.cs
├── Application/            # Business services and interfaces
├── Domain/                 # Category, Store, Product entities
├── Infrastructure/         # EF Core, repositories, migrations
├── UnitTests/              # Service, MVC and API controller tests
├── IntegrationTests/       # MVC and REST API HTTP tests
├── Postman/                # Exported Postman collection
└── README.md
```

The `Postman` directory is intended for the exported collection once it has been added to the repository.

## Architecture

CatalogManager follows a layered architecture that combines ASP.NET Core MVC and REST API.

The application provides two interfaces:

1. **MVC Controllers** return HTML pages rendered using Razor Views.
2. **API Controllers** return JSON responses to HTTP clients such as Postman.

Both interfaces reuse the same application services and business rules.

### Request Processing

**MVC request:**

Browser → MVC Controller → Application Service → Repository → EF Core / PostgreSQL

**REST API request:**

HTTP Client → API Controller → Application Service → Repository → EF Core / PostgreSQL

### Application Layers

- **Frontend:** MVC controllers, REST API controllers, Razor Views, ViewModels, and DTOs.
- **Application:** business services and interfaces.
- **Domain:** entities and domain-related definitions.
- **Infrastructure:** Entity Framework Core DbContext, repository implementations, and migrations.

Dependency Injection is used to connect controllers, services, and repositories.

### ViewModels and DTOs

The MVC application uses dedicated form ViewModels:

- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`

The REST API uses separate DTO classes for creating, updating, and returning data.

For example:

- `CategoryCreateDto`
- `CategoryUpdateDto`
- `CategoryResponseDto`

This approach separates user input and API contracts from the database entities.

### Data Model

CatalogManager contains three related entities:

- **Category:** contains multiple products.
- **Store:** contains multiple products.
- **Product:** belongs to one category and one store.

Each product contains the foreign keys `CategoryId` and `StoreId`.

The relationships restrict deletion of categories and stores that are still referenced by products.

---

## MVC Application (C1)

The MVC interface provides CRUD operations for Categories, Stores, and Products.

### Controllers

- `CategoriesController`
- `StoresController`
- `ProductsController`

Each controller provides the corresponding actions for displaying, creating, editing, and deleting records.

### Razor Views

Razor Views are stored in:

- `Frontend/Views/Categories`
- `Frontend/Views/Stores`
- `Frontend/Views/Products`

Bootstrap is used to style forms, navigation, tables, and buttons.

### MVC Validation

MVC ViewModels use Data Annotations, including:

- `[Required]`
- `[Range]`
- `[StringLength]`

The application also validates business rules at the service layer.

POST form actions use antiforgery validation to protect against CSRF attacks.

---

## REST API (C5)

CatalogManager provides a JSON-based REST API alongside the MVC application.

The API implements CRUD functionality for all three entities without using Razor Views.

### API Endpoints

| Entity | Method | Endpoint | Purpose |
|---|---|---|---|
| Categories | GET | `/api/categories` | Get all categories |
| Categories | GET | `/api/categories/{id}` | Get category by ID |
| Categories | POST | `/api/categories` | Create category |
| Categories | PUT | `/api/categories/{id}` | Update category |
| Categories | DELETE | `/api/categories/{id}` | Delete category |
| Stores | GET | `/api/stores` | Get all stores |
| Stores | GET | `/api/stores/{id}` | Get store by ID |
| Stores | POST | `/api/stores` | Create store |
| Stores | PUT | `/api/stores/{id}` | Update store |
| Stores | DELETE | `/api/stores/{id}` | Delete store |
| Products | GET | `/api/products` | Get all products |
| Products | GET | `/api/products/{id}` | Get product by ID |
| Products | POST | `/api/products` | Create product |
| Products | PUT | `/api/products/{id}` | Update product |
| Products | DELETE | `/api/products/{id}` | Delete product |

### API Controllers

The REST API is implemented in:

```text
Frontend/ApiControllers/
├── CategoriesApiController.cs
├── StoresApiController.cs
└── ProductsApiController.cs
```

API controllers inherit from `ControllerBase` and use:

- `[ApiController]`
- `[Route]`
- `[HttpGet]`
- `[HttpPost]`
- `[HttpPut]`
- `[HttpDelete]`

The controllers receive application services through constructor-based Dependency Injection.

### HTTP Status Codes

The API uses appropriate HTTP status codes:

| Status | Meaning |
|---|---|
| `200 OK` | Successful GET request |
| `201 Created` | Resource created successfully |
| `204 No Content` | Resource updated or deleted successfully |
| `400 Bad Request` | Invalid input or request data |
| `404 Not Found` | Requested resource does not exist |
| `409 Conflict` | Business constraint violation |

Examples of conflict scenarios include duplicate category names and attempts to delete categories or stores containing products.

### Example: Create Category

**Request:**

```http
POST /api/categories
Content-Type: application/json
```

**JSON body:**

```json
{
  "name": "Electronics",
  "description": "Electronic devices"
}
```

**Successful response:**

`201 Created`

```json
{
  "id": 1,
  "name": "Electronics",
  "description": "Electronic devices"
}
```

The response is an illustrative example. The actual ID is generated by the database, and creating a category with an existing name may produce a conflict.

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

The referenced category and store must exist before the product can be created.

### API Validation

The API validates incoming JSON using Data Annotations and business services.

Validation covers:

- Empty or missing required names.
- Invalid prices.
- Latitude outside the range −90 to 90.
- Longitude outside the range −180 to 180.
- Nonexistent category and store references.
- Duplicate category names.
- Attempts to delete referenced categories or stores.
- Nonexistent resource IDs.

The `[ApiController]` attribute enables automatic HTTP 400 responses for invalid model state.

---

## Postman API Testing

The REST API has been manually tested using Postman.

A Postman collection named **CatalogManager API Tests** contains requests for Categories, Stores, and Products.

### Tested Operations

- GET all resources.
- GET a resource by ID.
- POST new resources.
- PUT existing resources.
- DELETE resources.
- Verify resources after updates and deletions.

### Negative Test Scenarios

- Empty, missing, or invalid fields.
- Invalid JSON request bodies.
- Invalid coordinates.
- Zero and negative prices.
- Invalid category and store references.
- Nonexistent resource IDs.
- Duplicate category names.
- Deletion of categories or stores containing products.

### Postman Collection

The collection can be exported using:

Postman → Collections → CatalogManager API Tests → Export

The recommended repository location is:

`Postman/CatalogManager.postman_collection.json`

The exported collection can be imported into Postman to reproduce the API requests.

Automated collection execution and reporting are planned as part of **QA1 — Automated Web API Testing**.

---

## Requirements

- .NET 10 SDK
- PostgreSQL server
- Visual Studio with ASP.NET and web development tooling, or a compatible editor
- Postman for REST API testing
- Optional: pgAdmin for database inspection

## Local Setup

### 1. Clone and Restore

```bash
git clone <YOUR_REPOSITORY_URL>
cd CatalogManager
dotnet restore CatalogManager.slnx
```

### 2. Create the Application Database

Create a PostgreSQL database named `CatalogManagerDb`.

The default local setup uses:

- Host: `localhost`
- Port: `5432`
- Username: `postgres`

The committed `Frontend/appsettings.json` does not need to contain a database password.

Configure User Secrets for the MVC application:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend
```

Replace `YOUR_PASSWORD` with your local PostgreSQL password.

User Secrets are intended for local development and should not be used as a production secret store.

### 3. Apply EF Core Migrations

Install the EF Core command-line tool if necessary:

```bash
dotnet tool install --global dotnet-ef
```

Apply the migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project Frontend
```

### 4. Start the Application

```bash
dotnet run --project Frontend
```

Open the local application URL shown in the terminal.

The MVC interface provides navigation to Categories, Stores, and Products.

The REST API is available at the `/api/` endpoints.

For example:

```text
https://localhost:44361/api/categories
https://localhost:44361/api/stores
https://localhost:44361/api/products
```

Port `44361` is an example from the local IIS Express configuration and may differ on another machine.

Create at least one category and one store before creating a product.

---

## Testing

The solution contains two testing projects:

| Project | Purpose |
|---|---|
| `UnitTests` | Business service, MVC controller, and REST API controller tests using Moq |
| `IntegrationTests` | HTTP integration tests using WebApplicationFactory and a dedicated PostgreSQL database |

### Unit Tests

Unit tests verify individual components independently from the real database.

They cover:

- Category, Store, and Product services.
- MVC controllers.
- REST API controllers.
- Successful and unsuccessful CRUD operations.
- Data mapping.
- Validation and business exceptions.
- HTTP action results returned by API controllers.

Moq is used to replace dependencies with mock implementations.

### Integration Tests

Integration tests verify the interaction of application components through HTTP requests.

They cover:

- MVC page accessibility.
- MVC CRUD workflows.
- Antiforgery validation.
- REST API CRUD operations.
- JSON serialization and deserialization.
- HTTP response status codes.
- Database persistence.
- Validation of invalid requests.
- Referential integrity and deletion conflicts.

The tests use `WebApplicationFactory` to create a test instance of the ASP.NET Core application.

### Test Database Setup

Create a separate PostgreSQL database named:

`CatalogManagerTestDb`

The integration test factory checks the database name to prevent accidentally running tests against the main development database.

Configure User Secrets for the integration test project:

```bash
dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests
```

The integration test factory applies EF Core migrations to the test database.

### Run All Tests

From the solution root:

```bash
dotnet test CatalogManager.slnx
```

### Most Recently Confirmed Test Results

| Test Type | Passed |
|---|---:|
| Unit Tests | 190 |
| Integration Tests | 27 |
| **Total** | **217** |

**Result: 217 passed, 0 failed, 0 skipped.**

These are the last independently confirmed results from Visual Studio Test Explorer. Additional tests may have been added afterward.

---

## Code Coverage

Code coverage is collected using Coverlet.

Run:

```bash
dotnet test CatalogManager.slnx --collect:"XPlat Code Coverage"
```

Install ReportGenerator if necessary:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate a combined HTML report:

```bash
reportgenerator -reports:"UnitTests/TestResults/**/coverage.cobertura.xml;IntegrationTests/TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"
```

Open:

`CoverageReport/index.html`

Remove or exclude older coverage files before generating the report to avoid mixing outdated results.

### Previous Coverage Results

The last reviewed combined report, generated before the C5 changes, showed:

- **Line coverage:** 76.1%
- **Branch coverage:** 77.7%
- **Application layer line coverage:** 100%

These figures describe the previous MVC implementation and should not be treated as current C5 coverage results until a new report is generated.

---

## Security Notes

- Never commit PostgreSQL passwords, `secrets.json`, API keys, or other credentials.
- Database connection credentials are stored using User Secrets during local development.
- MVC form POST actions use antiforgery validation.
- MVC forms use dedicated ViewModels.
- REST API endpoints use dedicated DTOs.
- HTTP input validation is performed using Data Annotations and application services.
- The project is an educational application; additional security and deployment hardening may be required before public production use.

## Project Status

### Completed

- **C1 — MVC Application:** implemented and demonstrated.
- **C5 — REST API:** implemented for Categories, Stores, and Products.
- REST API CRUD manually verified using Postman.
- Automated unit and integration testing implemented for MVC and REST API.

### Next Stage

**QA1 — Automated Web API Testing**

The next development stage focuses on configuring repeatable automated Postman collection runs, validating responses with scripts, managing test data, and generating test execution results.

Future improvements may include API pagination, Docker deployment, cloud hosting, and additional frontend functionality.