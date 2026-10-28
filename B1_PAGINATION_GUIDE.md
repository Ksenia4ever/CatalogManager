# B1 — Pagination API (2 points)

**Assignment:** add paginated output for the C5 API, implementing `skip`, `limit` and `nextLink`.

## Endpoints

- `GET /api/categories?skip=0&limit=2`
- `GET /api/stores?skip=0&limit=2`
- `GET /api/products?skip=0&limit=2`

The query parameters are optional individually: `skip` defaults to `0`, `limit` defaults to `10` when at least one pagination parameter is present. Valid values: `skip >= 0`, `1 <= limit <= 100`. An invalid value results in HTTP `400 Bad Request`.

**Compatibility with C5 / QA1:** an endpoint called **without both** pagination parameters (`GET /api/products`) still returns the original JSON array. A paginated request returns the response envelope below. This allows existing Postman collections and unit tests to remain usable.

## Example response

`GET /api/categories?skip=0&limit=2`

```json
{
  "items": [
    { "id": 1, "name": "Books", "description": "Printed books" },
    { "id": 2, "name": "Electronics", "description": "Devices" }
  ],
  "skip": 0,
  "limit": 2,
  "totalCount": 5,
  "nextLink": "https://localhost:PORT/api/categories?skip=2&limit=2"
}
```

`nextLink` is `null` when no further items exist. Data is ordered by the entity ID for stable page boundaries. EF Core performs `CountAsync`, `Skip` and `Take` on the database query, avoiding loading all records before pagination. The old C5 methods and MVC views are unchanged.

## Defense demonstration (Postman)

1. Run CatalogManager locally and ensure the PostgreSQL database contains **at least 5** categories, stores or products; if there are fewer, add temporary sample records through the existing C5 API.
2. Send `GET {{baseUrl}}/api/categories?skip=0&limit=2`. Show 2 entries, `totalCount`, `skip`, `limit` and a non-null `nextLink`.
3. Follow the `nextLink`. Show that the next page contains different entries; continue until `nextLink` is `null`.
4. Repeat for `/api/stores` and `/api/products`.
5. Demonstrate `?skip=0&limit=0` and `?skip=-1&limit=2` produce HTTP 400.
6. Send `GET {{baseUrl}}/api/categories` without params to confirm the original C5 response is still an array.
7. Run the B1 tests in Visual Studio Test Explorer and show the results.

**Note:** the two requests used to fetch the page and the total count are separate queries; concurrent changes to the data between them can influence `totalCount` and the page. This is normal for simple offset pagination.

## Modified parts

- `Application/Models/PagedResult.cs` — page data and total count.
- `Application/Interfaces/*Service.cs`, `*Repository.cs` — paginated operations.
- `Application/Services/*Service.cs` — delegation to repositories.
- `Infrastructure/Repositories/*Repository.cs` — server-side EF Core pagination.
- `Frontend/DTOs/Pagination/PagedResponseDto.cs` — JSON API contract.
- `Frontend/ApiControllers/*ApiController.cs` — parameter validation, response, nextLink.
- `UnitTests/ApiControllers/*ApiControllerTests.cs` — B1 cases for all entity types.

## Run tests

```powershell
dotnet test CatalogManager.slnx
```

The archive is prepared in an environment without the .NET SDK, so this command must be run on your development machine. PostgreSQL integration tests require the separately configured test database/User Secrets described in `README.md`.
