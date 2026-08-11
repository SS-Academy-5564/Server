namespace Pulse.BL.Common.Pagination;

public sealed record PagedResult<T>
{
    public PagedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items;
        PageSize = pageSize;
        TotalCount = totalCount;

        PageNumber = TotalPages > 0
            ? Math.Clamp(pageNumber, 1, TotalPages)
            : 1;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
