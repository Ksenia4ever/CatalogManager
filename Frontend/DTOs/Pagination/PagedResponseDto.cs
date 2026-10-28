namespace CatalogManager.DTOs.Pagination;

public sealed record PagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int Skip,
    int Limit,
    int TotalCount,
    string? NextLink);