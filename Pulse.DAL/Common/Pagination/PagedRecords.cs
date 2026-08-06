namespace Pulse.DAL.Common.Pagination;

public sealed record PagedRecords<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize,int TotalCount);
