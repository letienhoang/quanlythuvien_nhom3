using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    public class SachQuaHansController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IFineCalculatorService _fineCalculator;

        public SachQuaHansController(LibraryDbContext context, IFineCalculatorService fineCalculator)
        {
            _context = context;
            _fineCalculator = fineCalculator;
        }

        // GET: SachQuaHans     
        public async Task<IActionResult> Index()
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            var reports = await _context.Database
                .SqlQueryRaw<SachQuaHanReportDto>(
                    "EXEC dbo.usp_GenerateReport @FromDate = {0}, @ToDate = {1}, @OnlyOverdue = {2}",
                    fromDate, toDate, true
                )
                .AsNoTracking()
                .ToListAsync();
            return View(reports);
        }

        // GET: SachQuaHans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.CuonSach)
                        .ThenInclude(cs => cs.Sach)
                .Include(p => p.PhieuPhats)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieuMuon == null)
                return NotFound();

            var today = DateTime.Today;
            var daysOverdue = (today - phieuMuon.HanTra.Date).Days;

            decimal overdayFine = 0;
            decimal damageFine = 0;
            decimal totalFine = 0;

            var bookFines = new Dictionary<int, decimal>();

            // 🔑 LẤY PHIẾU PHẠT (NẾU CÓ)
            var phieuPhat = phieuMuon.PhieuPhats?
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            if (phieuPhat != null)
            {
                // ĐÃ CÓ → LUÔN LẤY DB
                totalFine = phieuPhat.SoTienPhat;
                ViewBag.FineSource = "DB";
            }
            else
            {
                // ❗ CHƯA CÓ → TẠM TÍNH
                overdayFine = _fineCalculator.CalculateFine(phieuMuon);

                if (phieuMuon.ChiTietPhieuMuons != null)
                {
                    foreach (var chiTiet in phieuMuon.ChiTietPhieuMuons)
                    {
                        if (chiTiet.TinhTrangTra.HasValue &&
                            chiTiet.TinhTrangTra != ReturnCondition.NguyenVen)
                        {
                            decimal fine = _fineCalculator.CalculateDamageFine(
                                chiTiet.CuonSach,
                                chiTiet.TinhTrangTra.Value
                            );

                            bookFines[chiTiet.CuonSachId] = fine;
                            damageFine += fine;
                        }
                    }
                }

                totalFine = overdayFine + damageFine;
                ViewBag.FineSource = "CALCULATED";
            }

            ViewBag.DaysOverdue = daysOverdue;
            ViewBag.OverdayFine = overdayFine;
            ViewBag.DamageFine = damageFine;
            ViewBag.TotalFine = totalFine;
            ViewBag.BookFines = bookFines;

            return View(phieuMuon);
        }

        // POST: SachQuaHans/CreateFine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFine(int id, decimal soTienPhat, string lyDo)
        {
            var phieuMuon = await _context.PhieuMuons.FindAsync(id);
            if (phieuMuon == null)
                return NotFound();

            //Kiểm tra đã tạo phiếu phạt chưa 
            if (phieuMuon.PhieuPhats != null && phieuMuon.PhieuPhats.Count > 0)
            {
                TempData["Error"] = "Phiếu mượn này đã có phiếu phạt. Không thể tạo thêm!";
                return RedirectToAction(nameof(Details), new { id });
            }


            // Tạo phiếu phạt
            var phieuPhat = new PhieuPhat
            {
                MaPhat = $"PP{DateTime.Now:yyyyMMddHHmmss}",
                PhieuMuonId = id,
                SoTienPhat = soTienPhat,
                LyDo = lyDo,
                TrangThaiThanhToan = PaymentStatus.ChuaThanhToan
            };

            _context.PhieuPhats.Add(phieuPhat);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}