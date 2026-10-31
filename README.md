CatalogManager
CatalogManager is a layered ASP.NET Core MVC + REST API application for managing a product catalog. It supports categories, stores, and products, persists data in PostgreSQL using Entity Framework Core, and includes interactive maps, pagination, caching, data visualization, AJAX autocomplete, external currency exchange rates, a collaborative block-based notes editor with browser-based file import and export, and full-text product search powered by Lucene.NET.

The project includes automated unit, integration, Postman API, and Playwright E2E testing, Docker containerization, and GitHub Actions automation.

Developed for the Web Technologies coursework.
Completed Coursework Stages
Stage	Description	Points
C1	MVC application with CRUD operations	3
C5	REST API with DTOs and validation	3
QA1	Automated API testing with Postman	2
B1	REST API pagination	2
C8	Interactive stores map with Leaflet	3
B9	ASP.NET Core Output Caching	3
C4	Docker containerization	1
C6	Bootstrap product landing pages	3
C7	SEO optimization	1
F1	Data visualization with Chart.js	2
QA2	Playwright end-to-end testing	3
F2	AJAX autocomplete with Select2	4
B2	PrivatBank external API integration	2
C12	Dynamic notes editor	3
C13	Cross-browser synchronization with SignalR	2
C14	Collaborative change visualization	3
C15	Browser File API import and export	3
B4	Lucene.NET full-text product search API	3
Total	18 implemented stages	46


These are potential coursework points, not an officially awarded grade.
Features
Catalog Management
- CRUD operations for categories, stores, and products.
- Product-category and product-store relationships.
- Required-field and business-rule validation.
- Unique category names.
- Price and coordinate validation.
- Prevention of deleting referenced categories and stores.
- Optional product image URLs.
Web Interface
- ASP.NET Core MVC with Razor Views and Bootstrap.
- Responsive product catalog and individual landing pages.
- Interactive OpenStreetMap store markers.
- Chart.js statistics dashboard.
- AJAX category and store autocomplete.
- PrivatBank currency rates and approximate conversion.
- Block-based notes editor.
- Shared documents synchronized through SignalR.
- Visual highlighting of changes made in another browser.
- Recent remote changes history.
- Import JSON documents through the browser File API.
- Export shared documents as JSON or Markdown files.
- Search product names and descriptions through a Lucene.NET-powered full-text search page.
Infrastructure
- REST API with CRUD operations.
- Offset-based pagination.
- Output caching and invalidation.
- PostgreSQL persistence through EF Core.
- Docker Compose support.
- GitHub Actions automation.
- Server-side JSON persistence for shared notes.
- Document version checks for concurrent editing.
- On-demand Lucene.NET RAM index rebuilt from the current product catalog for each search request.
Testing
- xUnit and Moq.
- HTTP integration tests with WebApplicationFactory.
- Postman Collection Runner.
- Playwright with Chromium.
- Automated GitHub Actions E2E workflow.
Technology Stack
Backend
- C# / .NET 10
- ASP.NET Core MVC and Web API
- Razor Views
- Entity Framework Core 10
- PostgreSQL 17
- Npgsql
- ASP.NET Core SignalR
- ASP.NET Core Output Caching
- Lucene.NET 4.8 (Analysis.Common and QueryParser)
- HttpClient and IHttpClientFactory
- Dependency Injection
- System.Text.Json
Frontend
- HTML5
- CSS3
- JavaScript
- Bootstrap
- jQuery
- Select2
- AJAX / Fetch API
- Chart.js
- Leaflet.js
- SignalR JavaScript client
- Web Storage API / localStorage
- DOM manipulation
- CSS animations and transitions
- Browser File API, FileReader, Blob, and object URLs
External Services
- OpenStreetMap
- PrivatBank Public API
Testing and Development
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
- Chrome Lighthouse
- Git and GitHub
Solution Structure
```text

CatalogManager/

├── CatalogManager.slnx

├── .github/

│   └── workflows/

│       ├── c3-ci-cd.yml

│       └── qa2-playwright.yml

├── Frontend/

│   ├── Controllers/

│   │   ├── CategoriesController.cs

│   │   ├── StoresController.cs

│   │   ├── ProductsController.cs

│   │   ├── StoresMapController.cs

│   │   ├── StatisticsController.cs

│   │   ├── ProductAutocompleteController.cs

│   │   ├── CurrencyRatesController.cs

│   │   └── NotesEditorController.cs

│   ├── Hubs/

│   │   └── NotesHub.cs

│   ├── Services/

│   │   ├── ICurrencyRatesService.cs

│   │   ├── PrivatBankCurrencyRatesService.cs

│   │   └── SharedNotesStore.cs

│   ├── ApiControllers/

│   ├── Models/

│   ├── DTOs/

│   ├── Views/

│   │   ├── Categories/

│   │   ├── Stores/

│   │   ├── Products/

│   │   ├── StoresMap/

│   │   ├── Statistics/

│   │   ├── CurrencyRates/

│   │   ├── NotesEditor/

│   │   │   └── Index.cshtml

│   │   └── Shared/

│   │       └── _Layout.cshtml

│   ├── wwwroot/

│   │   ├── js/

│   │   │   ├── stores-map.js

│   │   │   ├── statistics.js

│   │   │   ├── product-autocomplete.js

│   │   │   ├── currency-rates.js

│   │   │   └── notes-editor.js

│   │   ├── css/

│   │   │   ├── stores-map.css

│   │   │   ├── product-landing.css

│   │   │   ├── statistics.css

│   │   │   ├── product-autocomplete.css

│   │   │   └── notes-editor.css

│   │   └── robots.txt

│   ├── App_Data/

│   │   └── SharedNotes/

│   │       └── (runtime JSON documents, not committed)

│   └── Program.cs

├── Application/

├── Domain/

├── Infrastructure/

├── UnitTests/

├── IntegrationTests/

│   └── Tests/

│       ├── StatisticsIntegrationTests.cs

│       ├── ProductAutocompleteIntegrationTests.cs

│       └── PrivatBankCurrencyRatesServiceTests.cs

├── E2ETests/

│   ├── CatalogManager.E2ETests.csproj

│   └── CatalogManagerBrowserTests.cs

├── Postman/

│   └── CatalogManager_QA1_B1_B9.postman_collection.json

├── Dockerfile

├── compose.yaml

├── .dockerignore

├── .env.example

├── .gitignore

├── B1_PAGINATION_GUIDE.md

└── README.md

```

The tree highlights the main project files. Other implementation files and tests may also exist.

Do not commit `.env`, generated build outputs, or shared note JSON files.
Architecture and Data Model
CatalogManager follows a layered architecture.
MVC Requests
```text

Browser

   |

   v

MVC Controller

   |

   v

Application Service

   |

   v

Repository

   |

   v

Entity Framework Core

   |

   v

PostgreSQL

```
REST API Requests
```text

HTTP Client

   |

   v

API Controller

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
Main Entities
Category: unique category name and associated products.

Store: name, address, latitude, longitude, and associated products.

Product: name, description, price, optional image URL, CategoryId, and StoreId.

Each product belongs to one category and one store.
MVC ViewModels
- `CategoryFormViewModel`
- `StoreFormViewModel`
- `ProductFormViewModel`
REST API actions use dedicated DTOs.
C1 — MVC Application
C1 implements Create, Read, Update, and Delete operations for Categories, Stores, and Products.

Features include:
- ASP.NET Core MVC controllers.
- Razor Views.
- Data Annotations.
- Application business rules.
- Antiforgery validation.
- Related entity selection.
- PostgreSQL persistence.
The MVC application provides the foundation for subsequent coursework stages.
C5 — REST API
The REST API provides JSON CRUD operations.
Entity	Collection Endpoint	Item Endpoint
Categories	`/api/categories`	`/api/categories/{id}`
Stores	`/api/stores`	`/api/stores/{id}`
Products	`/api/products`	`/api/products/{id}`


Supported methods:
- GET
- POST
- PUT
- DELETE
HTTP Status Codes
- `200 OK`: successful read.
- `201 Created`: successful creation.
- `204 No Content`: successful update or deletion.
- `400 Bad Request`: invalid input.
- `404 Not Found`: missing resource.
- `409 Conflict`: business rule violation.
Example Product Request
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

The supplied category and store IDs must exist.
QA1 — Automated Postman API Testing
QA1 includes automated positive and negative API test scenarios using Postman.

The original collection contains 50 requests:

1. Categories: 13 requests.

2. Stores: 13 requests.

3. Products: 15 requests.

4. Cleanup and Relationship Checks: 9 requests.
Automation
- JavaScript pre-request scripts.
- Automated response assertions.
- Collection variables.
- Unique test data.
- Positive and negative scenarios.
- Cleanup respecting foreign-key constraints.
Verified Results
Iterations	Passed Assertions	Failed
1	62	0
2	124	0


Verified on 28 October 2026.
B1 — Pagination API
B1 implements offset pagination.

```http

GET /api/categories?skip=0&limit=10

GET /api/stores?skip=0&limit=10

GET /api/products?skip=0&limit=10

```

Parameters:
- `skip`: number of records skipped.
- `limit`: maximum records returned.
- `nextLink`: link to the next page, if available.
Paginated responses contain:
- `items`
- `skip`
- `limit`
- `totalCount`
- `nextLink`
Requests without pagination parameters retain the original C5 array response.

Pagination is executed at the database query level.

Additional documentation: [B1_PAGINATION_GUIDE.md](B1_PAGINATION_GUIDE.md).
C8 — Interactive Stores Map
C8 integrates OpenStreetMap using Leaflet.js.

Page: `/StoresMap`

Data endpoint:

```http

GET /api/stores

```

Features:
- Interactive map.
- Store markers.
- Information popups.
- Store search.
- Visible marker counter.
- Refresh without page reload.
- Geographic coordinate validation.
Files
File	Purpose
`Frontend/Controllers/StoresMapController.cs`	MVC controller
`Frontend/Views/StoresMap/Index.cshtml`	Map page
`Frontend/wwwroot/js/stores-map.js`	JavaScript
`Frontend/wwwroot/css/stores-map.css`	CSS
`IntegrationTests/Tests/StoresMapIntegrationTests.cs`	Tests


Three integration test cases verify the map page and related resources.
B9 — API Output Caching
B9 uses ASP.NET Core Output Cache.
Policy	API	Duration	Tag
`CategoriesCache`	Categories	30 seconds	`categories`
`StoresCache`	Stores	30 seconds	`stores`
`ProductsCache`	Products	30 seconds	`products`


The application uses:
- `AddOutputCache()`
- `UseOutputCache()`
- `IOutputCacheStore.EvictByTagAsync()`
Successful API modifications invalidate the relevant cache tags.

Direct database modifications or MVC actions are not automatically covered by API-controller invalidation.
Verified Postman Results
Metric	Result
Requests	24
Assertions	48
Passed	48
Failed	0


Verified on 29 October 2026.

These tests verify API behavior and data visibility, not a separately measured cache HIT.
C4 — Docker Containerization
C4 runs the ASP.NET Core application and PostgreSQL using Docker Compose.
Architecture
```text

Windows Host

    |

    | http://localhost:8085

    v

Docker Compose

    |

    +--- web

    |    ASP.NET Core 10

    |    Port 8080

    |       |

    |       v

    +--- db

         PostgreSQL 17

         Port 5432

             |

             v

         Persistent Docker Volume

```

Web application: `http://localhost:8085`

PostgreSQL host port: `127.0.0.1:55432`
Start Containers
```powershell

docker compose up --build -d

```
Check Status
```powershell

docker compose ps -a

```

Expected:
- PostgreSQL: Healthy.
- Application: Up.
Database Migrations
For a new Docker database, configure the database connection and apply migrations.

```powershell

Env: Connectionstrings Defaultconnection = "Host=localhost;Port=55432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_DOCKER_PASSWORD"

dotnet ef database update --project Infrastructure --startup-project Frontend

Remove-Item Env:ConnectionStrings__DefaultConnection

```
Useful Commands
```powershell

docker compose restart

docker compose logs web --tail=80

docker compose down

docker compose up -d

```

Do not use `docker compose down -v` unless database deletion is intentional.
Verified Results
- Docker containers started successfully.
- PostgreSQL health checks passed.
- Database migrations were applied.
- MVC and API requests worked.
- Data survived container restarts.
- Docker was reused for QA2.
C6 — Bootstrap Product Landing Pages
C6 improves product presentation.

Catalog: `/Products`

Product page: `/Products/Details/{id}`

Features:
- Responsive product cards.
- Individual landing pages.
- Product names, descriptions, and prices.
- Optional images.
- Category and store information.
- Custom CSS.
Files
File	Purpose
`Frontend/Views/Products/Index.cshtml`	Catalog
`Frontend/Views/Products/Details.cshtml`	Product details
`Frontend/wwwroot/css/product-landing.css`	Styling


No new database migration was required.
C7 — SEO Optimization
C7 improves technical SEO.

Features:
- Product-specific page titles.
- Meta descriptions.
- Canonical URLs.
- Open Graph metadata.
- robots.txt.
- Chrome Lighthouse auditing.
Robots.txt
`Frontend/wwwroot/robots.txt`

```text

User-agent: *

Allow: /

```
Verified Lighthouse Results
Metric	Result
SEO	100/100
Passed Audits	9
Not Applicable	1
Failed Applicable Audits	0


The audit was performed on a local product details page on 30 October 2026.
F1 — Data Visualization
F1 adds a statistics dashboard using Chart.js.

Page: `/Statistics`

Endpoint:

```http

GET /Statistics/Data

```
Summary Indicators
- Total products.
- Total categories.
- Total stores.
Charts
1. Products by Category.

2. Products by Store.

3. Product Price Distribution.
Features
- Fetch API requests.
- Dynamic Chart.js rendering.
- Refresh button.
- Loading and error states.
- Responsive layout.
Files
File	Purpose
`Frontend/Controllers/StatisticsController.cs`	Controller
`Frontend/Views/Statistics/Index.cshtml`	Dashboard
`Frontend/wwwroot/js/statistics.js`	Chart logic
`Frontend/wwwroot/css/statistics.css`	Styling
`IntegrationTests/Tests/StatisticsIntegrationTests.cs`	Tests


Four integration tests were added.
Verified Results After F1
Test Type	Passed
Unit Tests	208
Integration Tests	59
Total	267


QA2 — End-to-End Testing with Playwright
QA2 introduces real-browser testing with Microsoft Playwright, Chromium, and xUnit.

Project: `E2ETests/CatalogManager.E2ETests.csproj`

Main test class: `E2ETests/CatalogManagerBrowserTests.cs`
Test Scenarios
Test	Scenario
QA2-01	Home page and Products navigation
QA2-02	Category Create, Edit, Delete
QA2-03	Required Category Name validation
QA2-04	Product catalog and Create form
QA2-05	Stores Map
QA2-06	Statistics
QA2-07	robots.txt


Configuration
Environment variable:

`CATALOGMANAGER_E2E_BASE_URL`

IIS Express: `https://localhost:44361`

Docker: `http://localhost:8085`
Install Playwright Chromium
```powershell

dotnet build E2ETests/CatalogManager.E2ETests.csproj

powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\E2ETests\bin\Debug\net10.0\playwright.ps1" install chromium

```
Run with IIS Express
Keep the application running.

```powershell

Env: Catalogmanager E2e Base URL = "https://localhost:44361"

dotnet test E2ETests/CatalogManager.E2ETests.csproj

```
Run with Docker
```powershell

docker compose up --build -d

Env: Catalogmanager E2e Base URL = "http://localhost:8085"

dotnet test E2ETests/CatalogManager.E2ETests.csproj

```
Verified Results
Environment	Total	Passed	Failed
IIS Express (earlier run)	7	7	0
Docker (earlier run)	7	7	0
GitHub Actions (verified run)	7	7	0


A subsequent local Test Explorer run produced `ERR_CONNECTION_REFUSED` for all seven E2E tests because the application was not reachable at the configured IIS Express address. The tests did not reach their browser assertions.

To run locally, first start the website and verify that the URL responds.
Functional Coverage
The seven E2E scenarios cover selected navigation, Category CRUD, validation, product form availability, Stores Map, Statistics, and robots.txt.

They do not provide full Store or Product CRUD browser coverage.
GitHub Actions — QA2 Automation
Workflow:

`.github/workflows/qa2-playwright.yml`
Triggers
- Pushes to `develop`.
- Pushes to `main`.
- Pull requests.
- Manual execution.
Workflow
1. Checkout source code.

2. Install .NET 10.

3. Start Docker Compose.

4. Prepare PostgreSQL.

5. Restore NuGet dependencies.

6. Apply EF Core migrations.

7. Build the test project.

8. Install Chromium.

9. Execute seven Playwright tests.

10. Upload test results.
Verified Result
Metric	Result
Workflow	QA2 - Playwright E2E Tests
Job	QA2 - Docker and Chromium
Branch	develop
Status	Success
Test Scenarios	7
Artifact	qa2-playwright-results


Verified on 31 October 2026.

To inspect the workflow, open:

GitHub → Actions → QA2 - Playwright E2E Tests

CI execution does not require local IIS Express.
F2 — AJAX Autocomplete
F2 adds server-side autocomplete to Product Create and Edit forms.

Pages:
- `/Products/Create`
- `/Products/Edit/{id}`
Technologies:
- jQuery
- Select2
- AJAX
- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
Behavior
- Search begins after three characters.
- Requests are delayed approximately 300 milliseconds.
- Backend filters matching records.
- Up to 20 results are returned.
- Existing MVC form submission is preserved.
Files
File	Purpose
`Frontend/Controllers/ProductAutocompleteController.cs`	Endpoints
`Frontend/wwwroot/js/product-autocomplete.js`	AJAX behavior
`Frontend/wwwroot/css/product-autocomplete.css`	Styling
`Frontend/Views/Products/Create.cshtml`	Create form
`Frontend/Views/Products/Edit.cshtml`	Edit form
`IntegrationTests/Tests/ProductAutocompleteIntegrationTests.cs`	Tests


Verified Results
Test Type	Before F2	After F2
Unit Tests	208	208
Integration Tests	59	65
Total	267	273


Six additional integration tests passed.
B2 — PrivatBank External API Integration
B2 integrates with the public PrivatBank currency exchange API.

Base address:

`https://api.privatbank.ua/`

Endpoint:

```http

GET /p24api/pubinfo?json&exchange&coursid=5

```

Pages:
- `/CurrencyRates`
- `/CurrencyRates/Index`
Features
- Server-side HttpClient requests.
- JSON parsing.
- Purchase and selling rates.
- Refresh button.
- Approximate currency converter.
- Error handling.
- Automated tests with controlled HTTP responses.
Conversion Formula
```text

Foreign currency amount = UAH amount / Bank selling rate

```

Example observed during testing:

```text

4000 UAH / 51.30 = 77.97 EUR

```

Rates may change, and calculations are indicative.
HttpClient Registration
```csharp

builder.Services.AddHttpClient<

    ICurrencyRatesService,

    PrivatBankCurrencyRatesService>(client =>

{

    client.BaseAddress = new Uri("https://api.privatbank.ua/");

    client.Timeout = TimeSpan.FromSeconds(10);

});

```
Implementation Files
File	Purpose
`Frontend/Services/ICurrencyRatesService.cs`	Interface
`Frontend/Services/PrivatBankCurrencyRatesService.cs`	Service
`Frontend/Controllers/CurrencyRatesController.cs`	Controller
`Frontend/Views/CurrencyRates/Index.cshtml`	Page
`Frontend/wwwroot/js/currency-rates.js`	JavaScript
`Frontend/Program.cs`	HttpClient registration
`IntegrationTests/Tests/PrivatBankCurrencyRatesServiceTests.cs`	Tests


Verified Results
Four B2 service tests passed:
- `ValidResponse_ParsesCurrencyRates`
- `ServerError_ThrowsHttpRequestException`
- `NoValidRates_ThrowsJsonException`
- `MalformedJson_ThrowsJsonException`
The currency page and converter were also manually verified.
C12 — Dynamic Notes Editor
Overview
C12 introduces a block-based notes editor.

The initial implementation uses JavaScript, DOM manipulation, and localStorage.

C13 subsequently extended the editor with server-side persistence, C14 added visualization of remote changes, and C15 added browser-based file import and export.
Editor Page
`/NotesEditor`
Supported Block Types
1. Heading 1.

2. Heading 2.

3. Paragraph.

4. Bulleted List.

5. Numbered List.
Features
- Add editable blocks.
- Edit text and block types.
- Delete blocks.
- Move blocks up and down.
- Preview formatted text.
- Automatically save document state.
- Restore previous content.
C12 Persistence
The original C12 document is stored using the browser's Web Storage API.

A localStorage entry is associated with the website origin and browser profile.

In C13, the editor gained an Import my C12 local notes button for transferring existing local notes into a server-side shared document.
Implementation Files
File	Purpose
`Frontend/Controllers/NotesEditorController.cs`	Controller
`Frontend/Views/NotesEditor/Index.cshtml`	View
`Frontend/wwwroot/js/notes-editor.js`	Block editing
`Frontend/wwwroot/css/notes-editor.css`	Styling
`Frontend/Views/Shared/_Layout.cshtml`	Navigation


Verified C12 Results
The editor was manually verified in Visual Studio and Chrome.

Testing covered creating, editing, moving, deleting, previewing, saving, and restoring blocks.

No PostgreSQL migration was required.

C12 result: implemented and manually verified.
C13 — Shared Notes Synchronization with SignalR
Overview
C13 extends the block editor with server-side persistence and synchronization across browser sessions.

Technologies:
- ASP.NET Core SignalR.
- SignalR JavaScript client.
- Server-side JSON files.
- Document identifiers.
- Version-based conflict detection.
Editor URLs
- `/NotesEditor`
- `/NotesEditor?doc=shared`
- `/NotesEditor?doc=testing`
Documents with different identifiers are independent.
Architecture
```text

Chrome / Browser A        Edge / Browser B

        |                        |

        +--------- SignalR ------+

                     |

                     v

                NotesHub

                     |

                     v

              SharedNotesStore

                     |

                     v

          App_Data/SharedNotes/

                 <id>.json

```
SignalR Methods
`JoinDocument(documentId)`:
- Validates the document ID.
- Joins the appropriate SignalR group.
- Returns the latest document snapshot.
`SaveDocument(documentId, expectedVersion, blocks)`:
- Validates the document.
- Checks the expected version.
- Saves the updated document.
- Sends `DocumentChanged` to other connections.
Server Storage
Service:

`Frontend/Services/SharedNotesStore.cs`

Local development folder:

`Frontend/App_Data/SharedNotes/`

Example:

`Frontend/App_Data/SharedNotes/shared.json`

Document IDs accept 1–64 Latin letters, digits, underscores, and hyphens.

Generated document data should not be committed to Git.
Version Control
Every document snapshot contains a version.

The browser provides the version it last received when saving.

If another browser has already saved a newer document, the server rejects the stale update.

This prevents silent overwrites but does not automatically merge simultaneous edits.
Implementation Files
File	Responsibility
`Frontend/Hubs/NotesHub.cs`	SignalR hub
`Frontend/Services/SharedNotesStore.cs`	JSON storage and version checks
`Frontend/Program.cs`	Service and hub registration
`Frontend/Views/NotesEditor/Index.cshtml`	Shared editor
`Frontend/wwwroot/js/notes-editor.js`	Client synchronization
`.gitignore`	Exclude generated JSON documents


Verified C13 Results
- Shared document opened in two browsers.
- SignalR connection established.
- Changes appeared in the second browser automatically.
- Editing worked in both directions.
- Documents were restored after restarting IIS Express.
- `shared` and `testing` remained independent.
No database migration was required.

C13 result: implemented and manually verified.
C14 — Collaborative Change Visualization
Overview
C14 extends the SignalR-based shared notes editor with visual feedback for document changes received from another browser.

It builds on C12 and C13 without replacing their existing functionality.

Coursework value: 3 points.
Page
`/NotesEditor?doc=shared`

The same document should be open in two browser sessions.
Technologies
- JavaScript.
- DOM manipulation.
- CSS animations.
- Bootstrap.
- ASP.NET Core SignalR.
- Document snapshot comparison.
Main Features
- Detect blocks added in another browser.
- Highlight remotely added blocks.
- Highlight remotely edited blocks.
- Show visual feedback for blocks moved remotely.
- Notify users about blocks deleted in another browser.
- Display recent remote changes.
- Synchronize document versions without reloading.
- Preserve existing editing, saving, preview, and reordering.
Visualization Architecture
```text

Browser A

    |

    | Edit / Add / Delete / Move block

    v

SignalR: SaveDocument

    |

    v

NotesHub

    |

    v

SharedNotesStore

    |

    v

Server Snapshot (new version)

    |

    | DocumentChanged

    v

Browser B

    |

    v

Compare previous and new snapshots

    |

    +--- Added block -> Green animation

    |

    +--- Edited block -> Yellow highlight

    |

    +--- Moved block -> Purple animation

    |

    +--- Deleted block -> Warning notification

    |

    v

Recent Remote Changes panel

```
Snapshot Comparison
The JavaScript code compares the previous document with the newly received snapshot.

Each block has:
- `id`
- `type`
- `text`
The block's position is determined by its index in the document array.

The comparison uses block IDs to identify changes.

A block is classified as:
- Added: ID exists in the new snapshot but not the previous one.
- Edited: ID exists in both snapshots, but text or type differs.
- Moved: block position differs and the block has not already been classified as edited.
- Deleted: ID exists in the previous snapshot but not the new one.
If a block is both edited and moved in one update, the current visualization prioritizes the edit classification.
Visual Effects
Change	Visualization
Added	Green highlight and `Added remotely` label
Edited	Yellow highlight and `Edited remotely` label
Moved	Purple animation and `Moved remotely` label
Deleted	Warning message showing the number of removed blocks


Visual markers are temporary and disappear automatically.
Recent Remote Changes
The editor contains a `Recent remote changes` panel.

The panel displays up to eight recent remote change messages in the current browser session.

Examples:

```text

Added: paragraph (position 3)

Edited: heading2 (position 3)

Moved: paragraph to position 1

Deleted: paragraph (previous position 2)

```

This is a client-side activity history, not a permanent server-side audit log.

The history is not intended to identify individual users.
Remote Updates Only
Visual effects are triggered when the editor receives a `DocumentChanged` event from another SignalR connection.

A user's own local changes are saved normally without being presented as remote changes.
Document ID Handling
The client validates document IDs before joining a SignalR group.

The latest JavaScript fix handles invalid or missing URL values.

Examples:
URL parameter	Result
No `doc` parameter	Opens `shared`
`?doc=shared`	Opens `shared`
`?doc=testing`	Opens `testing`
`?doc=null`	Falls back to `shared`
`?doc=undefined`	Falls back to `shared`


The `null` and `undefined` values are treated as invalid identifiers by the client.

The server-side `SharedNotesStore.ValidateId()` validation remains enabled.
Implementation Files
File	Responsibility
`Frontend/wwwroot/js/notes-editor.js`	Snapshot comparison, event history, visual effects, document ID handling
`Frontend/wwwroot/css/notes-editor.css`	Remote-change animations and highlighting
`Frontend/Views/NotesEditor/Index.cshtml`	Remote activity display


C14 reuses:
- `Frontend/Hubs/NotesHub.cs`
- `Frontend/Services/SharedNotesStore.cs`
- SignalR registration in `Frontend/Program.cs`
No new libraries or EF Core migrations were required.
Manual Verification
C14 was verified using Visual Studio, IIS Express, and two browser sessions.

Verification steps:

1. Open `/NotesEditor?doc=shared` in two browsers.

2. Confirm SignalR is connected.

3. Add a block in the first browser.

4. Verify that the second browser receives the new block.

5. Edit a block and observe the remote change activity.

6. Verify the recent changes panel.

7. Test changes to an existing heading.

8. Test the handling of an invalid document URL such as `?doc=null`.

9. Confirm that the editor continues to work with the server-side C13 document.

Observed results included:
- `Connected · live synchronization`.
- `Updated from another browser`.
- New server version numbers.
- `Recent remote changes` panel.
- Remote `Added` and `Edited` events.
- Existing document blocks preserved across updates.
The user also confirmed that the updated C14 implementation works after correcting the document ID handling.
Automated Testing Status
C14 changes are primarily client-side JavaScript and CSS.

No new automated C14 test count is claimed.

The latest Visual Studio Test Explorer screenshot showed:
Project	Passed
UnitTests	208
IntegrationTests	69
C# tests total	277


Seven local E2E tests failed during a separate run with `ERR_CONNECTION_REFUSED` because the application was unavailable at `https://localhost:44361`.

This is an environment/connection failure, not evidence of failed browser assertions.

The existing QA2 suite has previously passed using Docker and GitHub Actions.

A complete E2E regression run against the final C14 commit is recommended.
Limitations
- Remote change history is session-local.
- Visual effects are temporary.
- No persistent audit log is stored.
- No author identities or per-user cursors are displayed.
- Simultaneous edits are not merged automatically.
- C13 version conflict checks remain in effect.
- The editor has no document-level authentication or authorization.
- The shared editor is unsuitable for confidential information.
C14 Conclusion
C14 adds visual collaborative feedback to the shared notes editor.

It demonstrates:
- Comparing document snapshots.
- Detecting changes by block IDs.
- DOM updates and CSS animations.
- Reacting to SignalR notifications.
- Maintaining a client-side history of recent changes.
C14 result: implemented and manually verified.
C15 — Browser File API: Document Import and Export
Overview
C15 adds browser-based file handling to the collaborative Notes Editor. It extends the existing C12–C14 functionality without changing the ASP.NET Core controllers, SignalR hub, server-side storage or PostgreSQL schema.

Coursework value: 3 points.
Editor Page
/NotesEditor?doc=shared

The interface provides three file operations:
- Import JSON file — selects a local .json file and loads its blocks into the current shared document.
- Export JSON — downloads the current block document with its block IDs, types, and text, allowing later re-import.
- Export Markdown — downloads a readable .md file using Markdown headings, paragraphs, and lists. Markdown import is not part of C15.
Browser APIs and Technologies
- HTML file input and the browser's File API.
- FileReader for reading local file contents in JavaScript.
- JSON parsing and document structure validation.
- Blob and URL.createObjectURL() for creating downloadable files.
- DOM interaction and click events for importing and exporting.
- SignalR for distributing an imported document to other connected browsers after successful server-side saving.
Import Architecture
Local JSON file
      |
      v
Browser file picker / File API
      |
      v
FileReader -> JSON parse -> validate blocks
      |
      v
User confirms replacement
      |
      v
Notes Editor document state
      |
      v
SignalR SaveDocument
      |
      v
SharedNotesStore JSON persistence
      |
      v
DocumentChanged -> Other browsers
Export Architecture
Current editor blocks
      |
      +---- JSON serialization ----> JSON Blob
      |
      +---- Markdown formatting ---> Markdown Blob
                                        |
                                        v
                             Browser download via object URL
Import Validation and Safety
The browser implementation validates the selected file and its JSON structure before using the contents. It applies a file-size limit and asks the user to confirm before replacing the active shared document. Invalid or unsupported files should produce a visible error instead of silently replacing the document.

Importing into a shared document affects other connected browsers after saving. The existing C13 version checks still protect against silently overwriting a newer server version; simultaneous changes are not automatically merged.

For safety, imported document text is handled through the existing editor rendering and escaping logic rather than executed as HTML or JavaScript.
Implementation Files
File	Responsibility
Frontend/wwwroot/js/notes-editor.js	Browser file input, import, JSON/Markdown export, validation and integration with the shared editor
Frontend/Views/NotesEditor/Index.cshtml	Existing editor page; no additional Razor changes required for this C15 implementation
Frontend/Hubs/NotesHub.cs	Reused SignalR synchronization, unchanged
Frontend/Services/SharedNotesStore.cs	Reused server-side persistence, unchanged


No new .NET dependencies, PostgreSQL migrations or backend controllers were required.
Manual Verification
C15 was manually verified in the browser. The demonstrated workflow includes:
1. Open the shared notes editor with an existing document.
2. Export the current document as JSON and confirm the file is downloaded.
3. Open a separate test document, for example ?doc=c15test.
4. Import the exported JSON file, confirm replacement, and verify that its blocks appear.
5. Confirm that the imported changes reach another browser through SignalR.
6. Export Markdown and inspect the downloaded .md document.
7. Verify that invalid JSON is rejected without replacing the document.
8. Confirm that the existing editor, remote-change visualization, and server persistence still work.
The user confirmed that the C15 implementation works. No new automated C15 test count is claimed.
Limitations
- JSON import and JSON/Markdown export are supported; Markdown import is not included.
- The browser's File API does not provide unrestricted access to the local file system; a user must select a file for import.
- Import replaces the active document after confirmation, rather than merging block-by-block.
- A downloaded export is a snapshot, not an ongoing live link to the shared document.
- Shared documents remain unauthenticated in this coursework implementation and must not contain confidential data.
C15 Conclusion
C15 demonstrates practical browser file handling by reading local JSON files, validating imported data, creating downloadable JSON and Markdown content, and connecting the result to the existing SignalR-based collaborative editor.

C15 result: implemented and manually verified.
B4 — Full-Text Product Search with Lucene.NET
Overview
B4 adds full-text searching to the existing C5 product catalog REST API using Lucene.NET. It indexes product names and descriptions and returns matching products ranked by Lucene relevance score.

Coursework value: 3 points.
Technologies
- Lucene.Net (4.8.0-beta00018)
- Lucene.Net.Analysis.Common (4.8.0-beta00018)
- Lucene.Net.QueryParser (4.8.0-beta00018)
- ASP.NET Core MVC and Web API
- Existing IProductService and Entity Framework Core/PostgreSQL catalog data
Search API
GET /api/search/products?q=laptop
GET /api/search/products?q=laptop&limit=10
The API returns JSON search results. When a matching product is present, the response includes the product's ID, name, description, price, image URL, category ID, store ID and relevance score. The query is limited to 100 characters, and the requested result limit is clamped to the supported range of 1–50. An empty search returns no matches.
Search Page
/ProductSearch

The Razor page lets users enter search text and view product results with links to existing product details pages.
How the Search Works
HTTP request / ProductSearch page
            |
            v
ProductSearchApiController / ProductSearchController
            |
            v
ProductSearchService
            |
            +-- IProductService.GetAllAsync()
            |          |
            |          v
            |    PostgreSQL products
            |
            +-- Lucene RAMDirectory
            +-- StandardAnalyzer
            +-- IndexWriter (name and description)
            +-- MultiFieldQueryParser
            +-- IndexSearcher
            |
            v
Ranked product results
The service retrieves current products using the existing application service and constructs an in-memory Lucene index for each search request. Consequently, products created, updated, or deleted through the existing MVC/API flows are reflected in subsequent searches without requiring changes to existing CRUD controllers.

Trade-off: rebuilding the index for every search is appropriate for a small coursework catalog but inefficient for high traffic or large datasets. A production implementation would retain a persistent index and update it incrementally as products change.
Implementation Files
File	Responsibility
Frontend/Services/ProductSearchService.cs	Lucene index construction, analysis, search and result mapping
Frontend/ApiControllers/ProductSearchApiController.cs	JSON search endpoint
Frontend/Controllers/ProductSearchController.cs	MVC search page
Frontend/Views/ProductSearch/Index.cshtml	Search form and results
Frontend/Program.cs	Dependency injection and routing registration
Frontend/Views/Shared/_Layout.cshtml	Optional search navigation link
Frontend/CatalogManager.csproj	Lucene.NET NuGet package references


Manual Verification
The B4 implementation was checked locally and the user confirmed it works.
1. Open /ProductSearch.
2. Search for a term present in a product name or description.
3. Check the product results and product-detail links.
4. Open /api/search/products?q=laptop and inspect the JSON response; actual matches depend on the products in the local database.
5. Add or edit a product, then repeat a relevant search to verify the current catalog is used.
6. Search for a term with no matches and check the empty result.
No additional B4 automated-test count is claimed without a separately verified test run. No new PostgreSQL tables or EF Core migrations were required.
Limitations
- Each search rebuilds an in-memory index, which increases CPU usage as the catalog grows.
- Searches are based on the latest products returned by IProductService, not an independently stored Lucene index.
- Search behavior depends on tokenization and Lucene.NET analyzer rules.
- B4 adds search functionality but does not replace the existing F2 Select2 autocomplete.
B4 result: implemented and manually verified.
Postman Collection — QA1 + B1 + B9
File:

`Postman/CatalogManager_QA1_B1_B9.postman_collection.json`

The collection covers:
- QA1 automated API testing.
- B1 pagination.
- B9 output caching.
Run
1. Start CatalogManager.

2. Import the collection into Postman.

3. Set the `baseUrl` variable.

4. Select the relevant folder.

5. Use Collection Runner.

6. Review passed and failed assertions.

IIS Express: `https://localhost:44361`

Docker: `http://localhost:8085`

Use a test database for data-changing scenarios.
Local Setup Without Docker
Restore Dependencies
```powershell

dotnet restore CatalogManager.slnx

```
Configure PostgreSQL
Create `CatalogManagerDb` and configure User Secrets:

```powershell

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CatalogManagerDb;Username=postgres;Password=YOUR_PASSWORD" --project Frontend

```
Apply Migrations
```powershell

dotnet ef database update --project Infrastructure --startup-project Frontend

```
Run
Open the solution in Visual Studio.

Select IIS Express and press Ctrl + F5.

Alternatively:

```powershell

dotnet run --project Frontend

```
Main Pages
Feature	Route
Products	`/Products`
Create Product	`/Products/Create`
Stores Map	`/StoresMap`
Statistics	`/Statistics`
Currency Rates	`/CurrencyRates`
Notes Editor	`/NotesEditor`


Currency Rates requires external API availability.

Shared Notes uses SignalR and server-side JSON files.
Automated Testing
The solution contains:
- `UnitTests`
- `IntegrationTests`
- `E2ETests`
Integration Test Database
Use a separate database named `CatalogManagerTestDb`.

```powershell

dotnet user-secrets set "ConnectionStrings:TestConnection" "Host=localhost;Port=5432;Database=CatalogManagerTestDb;Username=postgres;Password=YOUR_PASSWORD" --project IntegrationTests

```
Unit Tests
```powershell

dotnet test UnitTests

```
Integration Tests
```powershell

dotnet test IntegrationTests

```
E2E Tests with IIS Express
Keep the application running:

```powershell

Env: Catalogmanager E2e Base URL = "https://localhost:44361"

dotnet test E2ETests/CatalogManager.E2ETests.csproj

```
E2E Tests with Docker
```powershell

docker compose up --build -d

Env: Catalogmanager E2e Base URL = "http://localhost:8085"

dotnet test E2ETests/CatalogManager.E2ETests.csproj

```
E2E Tests on GitHub
Open GitHub → Actions → QA2 - Playwright E2E Tests.

The workflow prepares the application, database, and browser automatically.
Latest Test Results
Test Type	Last Observed Result
Unit Tests	208 passed
Integration Tests	69 passed
Local Playwright E2E	7 connection failures in a later run
Earlier Playwright E2E / GitHub Actions	7 passed


A total of 277 C# tests passed in the latest observed local run.

The local QA2 connection failures should be resolved by running the tests while the application is accessible.

No new automated C12, C13, C14, C15, or B4 test count is claimed.
Code Coverage
Collect coverage:

```powershell

dotnet test UnitTests --collect:"XPlat Code Coverage"

dotnet test IntegrationTests --collect:"XPlat Code Coverage"

```

Install ReportGenerator if necessary:

```powershell

dotnet tool install --global dotnet-reportgenerator-globaltool

```

Generate the report:

```powershell

reportgenerator -reports:"UnitTests/TestResults//coverage.cobertura.xml;IntegrationTests/TestResults//coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;Cobertura"

```

Open:

`CoverageReport/index.html`

An earlier pre-C5 coverage report showed:
- Line coverage: 76.1%.
- Branch coverage: 77.7%.
- Application layer line coverage: 100%.
These are historical measurements, not current C15 coverage results.
Security Notes
- Never commit database credentials or secrets.
- Keep `.env` excluded from Git.
- Use User Secrets for local configuration.
- Use GitHub Secrets for sensitive CI configuration.
- Run data-changing tests only against development or test databases.
- MVC forms use antiforgery protection.
- REST API inputs use validation and business rules.
- robots.txt is not access control.
- Exchange rate calculations are indicative.
- Do not store passwords or confidential information in localStorage.
- Shared notes are not protected by document-level authorization.
- Do not commit `Frontend/App_Data/SharedNotes/` documents.
- Browser-side document ID validation does not replace server-side validation.
- JSON files are validated before import; never import untrusted confidential data.
- Exported files may contain the current notes; handle downloads appropriately.
- Production deployment requires additional security hardening.
Project Status
Implemented and Verified
- C1 — MVC Application.
- C5 — REST API.
- QA1 — Postman automated API tests.
- B1 — Pagination API.
- C8 — Interactive Stores Map.
- B9 — API Output Caching.
- C4 — Docker Containerization.
- C6 — Bootstrap Landing Pages.
- C7 — SEO Optimization.
- F1 — Chart.js Data Visualization.
- QA2 — Playwright E2E and GitHub Actions.
- F2 — AJAX Autocomplete.
- B2 — PrivatBank API.
- C12 — Dynamic Notes Editor.
- C13 — Shared Notes with SignalR.
- C14 — Collaborative Change Visualization.
- C15 — Browser File API import and JSON/Markdown export.
- B4 — Lucene.NET Full-Text Product Search API.