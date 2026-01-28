using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.ViewComponents;

public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int pageNumber, int pageSize, int totalItems,
        string actionName = "Index",
        int[]? pageSizeOptions = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSizeOptions == null || pageSizeOptions.Length == 0)
        {
            pageSizeOptions = new[] { 10, 20, 50, 100 };
        }

        var totalPages = (int)System.Math.Ceiling((double)totalItems / Math.Max(1, pageSize));
        var model = new PaginationModel
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            ActionName = actionName,
            PageSizeOptions = pageSizeOptions
        };

        // copy current query string to base route values (exclude page & pageSize)
        var query = HttpContext.Request.Query;
        var baseRoute = new RouteValueDictionary();
        foreach (var q in query)
        {
            var key = q.Key;
            if (string.Equals(key, "page", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "pageSize", System.StringComparison.OrdinalIgnoreCase))
                continue;
            // if multi-valued, keep first (or you can customize)
            baseRoute[key] = q.Value.ToString();
        }

        model.BaseRouteValues = baseRoute;
        return View(model);
    }
}

public class PaginationModel
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string ActionName { get; set; } = "Index";
    public int[] PageSizeOptions { get; set; } = new[] { 10, 20, 50, 100 };
    public RouteValueDictionary? BaseRouteValues { get; set; }
}