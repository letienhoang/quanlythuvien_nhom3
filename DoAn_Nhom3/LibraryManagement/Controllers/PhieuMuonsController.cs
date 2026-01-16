using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;
using LibraryManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Controllers
{
    public class PhieuMuonsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public PhieuMuonsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }

        // GET: PhieuMuons
        public async Task<IActionResult> Index()
        {
            var items = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .AsNoTracking()
                .ToListAsync();
            var soSachDangMuon = new Dictionary<int, int>();
            foreach (var item in items)
            {
                if (!soSachDangMuon.ContainsKey(item.NguoiMuonId))
                {
                    var soSach = await _context.Database
                        .SqlQuery<int>($"SELECT dbo.fn_TinhSoNgayTre({item.NguoiMuonId}) AS Value")
                        .FirstOrDefaultAsync();

                    soSachDangMuon[item.NguoiMuonId] = soSach;
                }
            }
            ViewBag.SoSachDangMuon = soSachDangMuon;

            return View(items);
        }

        // GET: PhieuMuons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons!)
                    .ThenInclude(ct => ct.CuonSach)
                        .ThenInclude(cs => cs!.Sach)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuMuon == null) return NotFound();

            // Lấy danh sách cuốn sách có sẵn (chưa được mượn)
            var cuonSachCoSan = await _context.CuonSachs
                .Include(c => c.Sach)
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CuonSachCoSan = cuonSachCoSan;

            return View(phieuMuon);
        }

        // POST: PhieuMuons/ThemSach - Thêm sách vào phiếu mượn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSach(int phieuMuonId, int[] cuonSachIds)
        {
            if (cuonSachIds == null || cuonSachIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một cuốn sách.";
                return RedirectToAction(nameof(Details), new { id = phieuMuonId });
            }

            var phieuMuon = await _context.PhieuMuons.FindAsync(phieuMuonId);
            if (phieuMuon == null) return NotFound();

            foreach (var cuonSachId in cuonSachIds)
            {
                // Kiểm tra sách đã có trong phiếu mượn chưa
                var existing = await _context.ChiTietPhieuMuons
                    .AnyAsync(ct => ct.PhieuMuonId == phieuMuonId && ct.CuonSachId == cuonSachId);

                if (!existing)
                {
                    // Thêm chi tiết phiếu mượn
                    var chiTiet = new ChiTietPhieuMuon
                    {
                        PhieuMuonId = phieuMuonId,
                        CuonSachId = cuonSachId,
                        NgayTra = null,
                        TinhTrangTra = null
                    };
                    _context.ChiTietPhieuMuons.Add(chiTiet);

                    // Cập nhật trạng thái cuốn sách thành "Đang mượn"
                    var cuonSach = await _context.CuonSachs.FindAsync(cuonSachId);
                    if (cuonSach != null)
                    {
                        cuonSach.TrangThai = CopyStatus.DangMuon;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm {cuonSachIds.Length} cuốn sách vào phiếu mượn.";

            return RedirectToAction(nameof(Details), new { id = phieuMuonId });
        }

        // POST: PhieuMuons/XoaSach - Xóa sách khỏi phiếu mượn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaSach(int phieuMuonId, int cuonSachId)
        {
            var chiTiet = await _context.ChiTietPhieuMuons
                .FirstOrDefaultAsync(ct => ct.PhieuMuonId == phieuMuonId && ct.CuonSachId == cuonSachId);

            if (chiTiet != null)
            {
                _context.ChiTietPhieuMuons.Remove(chiTiet);

                // Cập nhật trạng thái cuốn sách thành "Có sẵn"
                var cuonSach = await _context.CuonSachs.FindAsync(cuonSachId);
                if (cuonSach != null)
                {
                    cuonSach.TrangThai = CopyStatus.CoSan;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sách khỏi phiếu mượn.";
            }

            return RedirectToAction(nameof(Details), new { id = phieuMuonId });
        }

        // GET: PhieuMuons/Create
        public async Task<IActionResult> Create()
        {
            var model = new PhieuMuon
            {
                MaPhieuMuon = await _codeGen.GenerateNextAsync<PhieuMuon>(p => p.MaPhieuMuon, "PM", 4),
                NgayMuon = DateTime.Now,
                HanTra = DateTime.Now.AddDays(14)
            };

            await PopulateSelectsAsync();
            ViewBag.GeneratedMaPhieuMuon = model.MaPhieuMuon;
            return View(model);
        }

        // POST: PhieuMuons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuMuon,NguoiMuonId,NhanVienId,NgayMuon,HanTra,TrangThai,SoNgayTre")] PhieuMuon phieuMuon)
        {
            // ensure unique code with retries
            phieuMuon.MaPhieuMuon = await _codeGen.GenerateNextWithRetriesAsync<PhieuMuon>(p => p.MaPhieuMuon, "PM", 4);

            if (ModelState.IsValid)
            {
                _context.Add(phieuMuon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectsAsync();
            ViewBag.GeneratedMaPhieuMuon = phieuMuon.MaPhieuMuon;
            return View(phieuMuon);
        }

        // GET: PhieuMuons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons!)
                    .ThenInclude(ct => ct.CuonSach)
                        .ThenInclude(cs => cs!.Sach)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (phieuMuon == null) return NotFound();

            await PopulateSelectsAsync(phieuMuon.NguoiMuonId, phieuMuon.NhanVienId);
            return View(phieuMuon);
        }

        // POST: PhieuMuons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaPhieuMuon,NguoiMuonId,NhanVienId,NgayMuon,HanTra,TrangThai,SoNgayTre")] PhieuMuon phieuMuon)
        {
            if (id != phieuMuon.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuMuon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuMuonExists(phieuMuon.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectsAsync(phieuMuon.NguoiMuonId, phieuMuon.NhanVienId);
            return View(phieuMuon);
        }

        // GET: PhieuMuons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuMuon == null) return NotFound();

            return View(phieuMuon);
        }

        // POST: PhieuMuons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuMuon = await _context.PhieuMuons.FindAsync(id);
            if (phieuMuon != null)
            {
                _context.PhieuMuons.Remove(phieuMuon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuMuonExists(int id) =>
            _context.PhieuMuons.Any(e => e.Id == id);

        private async Task PopulateSelectsAsync(int? selectedNguoiId = null, int? selectedNhanVienId = null)
        {
            var nguoiList = await _context.NguoiMuons
                .AsNoTracking()
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(n.HoTen) ? n.MaNguoiMuon : $"{n.HoTen} ({n.MaNguoiMuon})"
                })
                .ToListAsync();

            var nhanvienList = await _context.NhanViens
                .AsNoTracking()
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(n.HoTen) ? n.MaNhanVien : $"{n.HoTen} ({n.MaNhanVien})"
                })
                .ToListAsync();

            ViewBag.NguoiMuonId = new SelectList(nguoiList, "Value", "Text", selectedNguoiId?.ToString());
            ViewBag.NhanVienId = new SelectList(nhanvienList, "Value", "Text", selectedNhanVienId?.ToString());
        }

        // Function In SQL 
        public async Task<int> GetSoSachDangMuon(int nguoiMuonId)
        {
            var result = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_TinhSoNgayTre({nguoiMuonId})")
                .FirstOrDefaultAsync();

            return result;
        }

        // POST: PhieuMuons/TraSach - Cập nhật tình trạng trả sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraSach(int phieuMuonId, int cuonSachId, DateTime ngayTra, ReturnCondition tinhTrangTra)
        {
            var chiTiet = await _context.ChiTietPhieuMuons
                .FirstOrDefaultAsync(ct => ct.PhieuMuonId == phieuMuonId && ct.CuonSachId == cuonSachId);

            if (chiTiet == null) return NotFound();

            // Cập nhật chi tiết phiếu mượn
            chiTiet.NgayTra = ngayTra;
            chiTiet.TinhTrangTra = tinhTrangTra;

            // Cập nhật trạng thái cuốn sách
            var cuonSach = await _context.CuonSachs.FindAsync(cuonSachId);
            if (cuonSach != null)
            {
                // Nếu sách bị mất hoặc hỏng nặng
                if (tinhTrangTra == ReturnCondition.Mat)
                {
                    cuonSach.TrangThai = CopyStatus.BaoTri; // Hoặc status khác
                    cuonSach.TinhTrang = BookCondition.Mat;
                }
                else if (tinhTrangTra == ReturnCondition.Hong)
                {
                    cuonSach.TrangThai = CopyStatus.BaoTri;
                    cuonSach.TinhTrang = BookCondition.Hong;
                }
                else
                {
                    cuonSach.TrangThai = CopyStatus.CoSan; // Trả về có sẵn
                }
            }

            // Kiểm tra nếu tất cả sách đã trả -> cập nhật trạng thái phiếu mượn
            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.Id == phieuMuonId);

            if (phieuMuon != null)
            {
                var tatCaDaTra = phieuMuon.ChiTietPhieuMuons?.All(ct => ct.NgayTra.HasValue) ?? false;
                if (tatCaDaTra)
                {
                    phieuMuon.TrangThai = LoanStatus.DaTraDu;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật tình trạng trả sách thành công.";

            return RedirectToAction(nameof(Edit), new { id = phieuMuonId });
        }
    }
}
