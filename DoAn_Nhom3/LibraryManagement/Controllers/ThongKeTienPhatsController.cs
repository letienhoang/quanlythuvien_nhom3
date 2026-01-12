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

        public IActionResult Index(int? thang, int? nam)
        {
            int month = thang ?? DateTime.Now.Month;
            int year = nam ?? DateTime.Now.Year;

            var phieuMuons = _context.PhieuMuons
           .Include(p => p.NguoiMuon)
           .Include(p => p.ChiTietPhieuMuons)
           .Where(p =>
               p.NgayMuon.Month == month &&
               p.NgayMuon.Year == year &&
               p.HanTra < DateTime.Now
           )
           .OrderByDescending(p => p.HanTra)
           .ToList();

            decimal tongTienPhat = phieuMuons
                .Sum(pm => _fineCalculatorService.CalculateTotalFine(pm));

            ViewBag.Thang = month;
            ViewBag.Nam = year;
            ViewBag.TongTienPhat = tongTienPhat;

            return View(phieuMuons);
        }
    }
}
