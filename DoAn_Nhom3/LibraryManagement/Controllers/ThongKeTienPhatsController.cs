using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagement.Controllers
{
    public class ThongKeTienPhatsController : Controller
    {
        private  readonly LibraryDbContext _context;
        private readonly IFineCalculatorService _fineCalculatorService;


        public ThongKeTienPhatsController(LibraryDbContext context, IFineCalculatorService fineCalculatorService)
        {
            _context = context;
            _fineCalculatorService = fineCalculatorService;
        }

        public async Task<IActionResult> Index(int? thang, int? nam)
        {
            var query = _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.ChiTietPhieuMuons)
                .Where(p => p.HanTra < DateTime.Now)
                .AsQueryable();

            // Nếu có lọc tháng / năm
            if (thang.HasValue && nam.HasValue)
            {
                query = query.Where(p =>
                    p.HanTra.Month == thang &&
                    p.HanTra.Year == nam);
            }

            var data = await query.ToListAsync();

            // Tổng tiền phạt
            decimal tongTien = data.Sum(p => _fineCalculatorService.CalculateTotalFine(p));

            // Dữ liệu biểu đồ (group theo tháng)
            var chartData = data
                .GroupBy(p => p.HanTra.Month)
                .Select(g => new
                {
                    Thang = g.Key,
                    TongTien = g.Sum(p => _fineCalculatorService.CalculateTotalFine(p))
                })
                .OrderBy(x => x.Thang)
                .ToList();

            ViewBag.Thang = thang;
            ViewBag.Nam = nam;
            ViewBag.TongTienPhat = tongTien;
            ViewBag.ChartLabels = chartData.Select(x => $"Tháng {x.Thang}").ToList();
            ViewBag.ChartValues = chartData.Select(x => x.TongTien).ToList();

            return View(data);
        }
    }
}
