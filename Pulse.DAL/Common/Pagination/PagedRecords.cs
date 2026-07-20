namespace Pulse.DAL.Common.Pagination;

public sealed record PagedRecords<T>(IReadOnlyList<T> Items, int TotalCount);
