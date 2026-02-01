using LibraryManagement.DtosModels;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    public class ThongKeMuonController : Controller
    {
        private readonly LibraryDbContext _context;

        public ThongKeMuonController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: /ThongKeMuons/Index
        // optional: thang, nam
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, bool? onlyOverdue)
        {
            // gọi SP trả result set khớp với BorrowReportItem
            var items = await _context.Database
                .SqlQuery<BorrowReportItem>($"""
                                                 EXEC usp_GenerateReport
                                                     {fromDate},
                                                     {toDate},
                                                     {onlyOverdue}
                                             """)
                .ToListAsync();

            // truyền lại giá trị để view giữ state
            BorrowReportViewModel vm = new BorrowReportViewModel()
            {
                FromDate = fromDate ?? DateTime.Now.AddMonths(-1),
                ToDate = toDate ?? DateTime.Now,
                OnlyOverdue = onlyOverdue ?? false,
                Items = items
            };

            // tính tổng (nếu cần hiển thị)
            ViewBag.TongLuotMuon = items.Count;
            ViewBag.TongSachMuon = items.Sum(i => i.SoSachDangMuon);

            return View(vm);
        }
    }
}
