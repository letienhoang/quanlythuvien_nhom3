using LibraryManagement.DtosModels;

namespace LibraryManagement.ViewModels;

public class BorrowReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool OnlyOverdue { get; set; }
    
    public List<BorrowReportItem> Items { get; set; } = new();
}