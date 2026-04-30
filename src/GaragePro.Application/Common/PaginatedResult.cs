namespace GaragePro.Application.Common;

public sealed class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }

    public PaginatedResult(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
    }
}
