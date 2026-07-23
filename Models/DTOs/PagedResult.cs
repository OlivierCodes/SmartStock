namespace SmartStock.Models.DTOs;

/// <summary>
/// Résultat paginé générique.
/// </summary>
/// <typeparam name="T">Type des éléments de la page.</typeparam>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
