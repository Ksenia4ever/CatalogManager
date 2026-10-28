namespace CatalogManager.Application.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);