namespace LibraryManagement.ViewModels;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; } = 0;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, PageSize));
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}