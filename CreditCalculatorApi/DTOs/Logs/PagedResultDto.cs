namespace CreditCalculatorApi.DTOs.Logs
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public long TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedResultDto() { }

        public PagedResultDto(IReadOnlyList<T> items, long totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
