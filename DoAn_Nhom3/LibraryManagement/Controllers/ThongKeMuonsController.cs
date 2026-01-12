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
        public async Task<IActionResult> Index(int? thang, int? nam)
        {
            var query = _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.ChiTietPhieuMuons)
                .AsQueryable();

            // 🔥 CHỈ lọc khi người dùng bấm thống kê
            if (thang.HasValue && nam.HasValue)
            {
                query = query.Where(p =>
                    p.NgayMuon.Month == thang.Value &&
                    p.NgayMuon.Year == nam.Value);
            }

            var data = await query
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            // Tổng số lượt mượn & tổng sách
            ViewBag.TongLuotMuon = data.Count;
            ViewBag.TongSachMuon = data.Sum(p => p.ChiTietPhieuMuons.Count);

            ViewBag.Thang = thang;
            ViewBag.Nam = nam;

            return View(data);
        }
    }
}
