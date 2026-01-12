using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    public class ThongKeMuonsController : Controller
    {
        private readonly LibraryDbContext _context;

        public ThongKeMuonsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: /ThongKe/MuonTheoThang
        public IActionResult Index(int? thang, int? nam)
        {
            int month = thang ?? DateTime.Now.Month;
            int year = nam ?? DateTime.Now.Year;
            // ngày bắt đầu mượn
            var phieuMuons = _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.ChiTietPhieuMuons)
                .Where(p => p.NgayMuon.Month == month &&
                            p.NgayMuon.Year == year)
                .OrderByDescending(p => p.NgayMuon)
                .ToList();

            ViewBag.Thang = month;
            ViewBag.Nam = year;

            return View(phieuMuons);
        }
    }
}
