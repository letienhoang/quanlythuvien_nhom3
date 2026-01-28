using LibraryManagement.DtosModels;
using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    public class PhieuMuonsController : Controller
    {
        private readonly LibraryDbContext _context;

        public PhieuMuonsController(LibraryDbContext context)
        {
            _context = context;
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
                if (!soSachDangMuon.ContainsKey(item.MaNguoiMuon))
                {
                    //Funtion số sách đang mượn 
                    var soSach = await _context.Database
                        .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({item.MaNguoiMuon}) AS Value")
                        .FirstOrDefaultAsync();

                    soSachDangMuon[item.MaNguoiMuon] = soSach;
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
                .FirstOrDefaultAsync(m => m.MaNguoiMuon == id);

            if (phieuMuon == null) return NotFound();

            // Lấy danh sách cuốn sách có sẵn (chưa được mượn)
            var cuonSachCoSan = await _context.CuonSachs
                .Include(c => c.Sach)
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CuonSachCoSan = cuonSachCoSan;

            // Tính số sách đang mượn của độc giả (dùng hàm SQL) và số slot còn lại
            var soSachDangMuon = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({phieuMuon.MaNguoiMuon}) AS Value")
                .FirstOrDefaultAsync();

            var remainingSlots = Math.Max(0, 3 - soSachDangMuon);
            ViewBag.RemainingSlots = remainingSlots;

            return View(phieuMuon);
        }

        // POST: PhieuMuons/ThemSach - Thêm sách vào phiếu mượn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSach(int maPhieuMuon, int[] MaCuons)
        {
            if (MaCuons == null || MaCuons.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một cuốn sách.";
                return RedirectToAction(nameof(Details), new { maPhieuMuon = maPhieuMuon });
            }

            var phieuMuon = await _context.PhieuMuons.FindAsync(maPhieuMuon);
            if (phieuMuon == null) return NotFound();

            // Kiểm tra số slot còn lại (server-side) bằng hàm fn_SoSachDangMuon
            var soSachDangMuon = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({phieuMuon.MaNguoiMuon}) AS Value")
                .FirstOrDefaultAsync();

            var remainingSlots = Math.Max(0, 3 - soSachDangMuon);
            if (remainingSlots <= 0)
            {
                TempData["Error"] = "Độc giả đã mượn tối đa 3 cuốn.";
                return RedirectToAction(nameof(Details), new { maPhieuMuon = maPhieuMuon });
            }

            if (MaCuons.Length > remainingSlots)
            {
                TempData["Error"] = $"Bạn chỉ có thể thêm tối đa {remainingSlots} cuốn nữa.";
                return RedirectToAction(nameof(Details), new { maPhieuMuon = maPhieuMuon });
            }

            foreach (var MaCuon in MaCuons)
            {
                // Kiểm tra sách đã có trong phiếu mượn chưa
                var existing = await _context.ChiTietPhieuMuons
                    .AnyAsync(ct => ct.MaPhieuMuon == maPhieuMuon && ct.MaCuon == MaCuon);

                if (!existing)
                {
                    // Thêm chi tiết phiếu mượn
                    var chiTiet = new ChiTietPhieuMuon
                    {
                        MaPhieuMuon = maPhieuMuon,
                        MaCuon = MaCuon,
                        NgayTra = null,
                        TinhTrangTra = null
                    };
                    _context.ChiTietPhieuMuons.Add(chiTiet);

                    // Cập nhật trạng thái cuốn sách thành "Đang mượn"
                    var cuonSach = await _context.CuonSachs.FindAsync(MaCuon);
                    if (cuonSach != null)
                    {
                        cuonSach.TrangThai = CopyStatus.DangMuon;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm {MaCuons.Length} cuốn sách vào phiếu mượn.";

            return RedirectToAction(nameof(Details), new { maPhieuMuon = maPhieuMuon });
        }

        // POST: PhieuMuons/XoaSach - Xóa sách khỏi phiếu mượn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaSach(int maPhieuMuon, int MaCuon)
        {
            var chiTiet = await _context.ChiTietPhieuMuons
                .FirstOrDefaultAsync(ct => ct.MaPhieuMuon == maPhieuMuon && ct.MaCuon == MaCuon);

            if (chiTiet != null)
            {
                _context.ChiTietPhieuMuons.Remove(chiTiet);

                // Cập nhật trạng thái cuốn sách thành "Có sẵn"
                var cuonSach = await _context.CuonSachs.FindAsync(MaCuon);
                if (cuonSach != null)
                {
                    cuonSach.TrangThai = CopyStatus.CoSan;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sách khỏi phiếu mượn.";
            }

            return RedirectToAction(nameof(Details), new { maPhieuMuon = maPhieuMuon });
        }

        // GET: PhieuMuons/Create
        public async Task<IActionResult> Create()
        {
            var dto = new CreateBorrowRecordDto
            {
                HanTra = DateTime.Now.AddDays(14)
            };

            ViewBag.MaNguoiMuon = new SelectList(await _context.NguoiMuons.ToListAsync(), "MaNguoiMuon", "HoTen");
            ViewBag.MaNhanVien = new SelectList(await _context.NhanViens.ToListAsync(), "MaNhanVien", "HoTen");

            var cuonSachCoSan = await _context.CuonSachs
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .Include(c => c.Sach)
                .ToListAsync();

            ViewBag.CuonSachCoSan = cuonSachCoSan;

            return View(dto);

        }

        // POST: PhieuMuons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBorrowRecordDto dto, int[] maCuons)
        {
            if (maCuons == null || maCuons.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một cuốn sách.");
            }
            if (maCuons.Length > 3)
            {
                ModelState.AddModelError("", "Chỉ được mượn tối đa 3 cuốn sách.");
            }

            // Kiểm tra số sách đang mượn hiện tại của độc giả
            var soSachDangMuon = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({dto.MaNguoiMuon}) AS Value")
                .FirstOrDefaultAsync();

            if (soSachDangMuon + maCuons.Length > 3)
            {
                ModelState.AddModelError("", $"Độc giả đã mượn {soSachDangMuon} cuốn, chỉ được mượn tối đa 3 cuốn. Vui lòng trả bớt sách trước khi mượn thêm.");
            }

            if (ModelState.IsValid)
            {
                var danhSachMaCuon = string.Join(",", maCuons);

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC usp_CreateBorrowRecord {0}, {1}, {2}, {3}, {4}",
                        dto.MaPhieuMuon,
                        dto.MaNguoiMuon,
                        dto.MaNhanVien,
                        danhSachMaCuon,
                        dto.HanTra
                    );
                    TempData["Success"] = $"Đã tạo phiếu mượn với {maCuons.Length} cuốn sách.";
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tạo phiếu mượn: {ex.Message}");
                }
            }

            // Load lại danh sách nếu lỗi
            await LoadDropdowns(dto.MaNguoiMuon, dto.MaNhanVien);
            ViewBag.CuonSachCoSan = await _context.CuonSachs
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .Include(c => c.Sach)
                .ToListAsync();
            return View(dto);
        }

        // GET: PhieuMuons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons!)
                    .ThenInclude(ct => ct.CuonSach)
                        .ThenInclude(cs => cs!.Sach)
                .FirstOrDefaultAsync(m => m.MaPhieuMuon == id);
                
            if (phieuMuon == null) return NotFound();

            await PopulateSelectsAsync(phieuMuon.MaNguoiMuon, phieuMuon.MaNhanVien);
            return View(phieuMuon);
        }

        // POST: PhieuMuons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maPhieuMuon, [Bind("MaNguoiMuon,MaNhanVien,NgayMuon,HanTra,TrangThai,SoNgayTre")] PhieuMuon phieuMuon)
        {
            if (maPhieuMuon != phieuMuon.MaPhieuMuon) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuMuon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuMuonExists(phieuMuon.MaPhieuMuon))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectsAsync(phieuMuon.MaNguoiMuon, phieuMuon.MaNhanVien);
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
                .FirstOrDefaultAsync(m => m.MaPhieuMuon == id);

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
            _context.PhieuMuons.Any(e => e.MaPhieuMuon == id);

        private async Task PopulateSelectsAsync(int? selectedNguoiId = null, int? selectedMaNhanVien = null)
        {
            var nguoiList = await _context.NguoiMuons
                .AsNoTracking()
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.MaNguoiMuon.ToString(),
                    Text = string.IsNullOrWhiteSpace(n.HoTen) ? n.MaNguoiMuon.ToString() : $"{n.HoTen} ({n.MaNguoiMuon})"
                })
                .ToListAsync();

            var nhanvienList = await _context.NhanViens
                .AsNoTracking()
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.MaNhanVien.ToString(),
                    Text = string.IsNullOrWhiteSpace(n.HoTen) ? n.MaNhanVien.ToString() : $"{n.HoTen} ({n.MaNhanVien})"
                })
                .ToListAsync();

            ViewBag.MaNguoiMuon = new SelectList(nguoiList, "Value", "Text", selectedNguoiId?.ToString());
            ViewBag.MaNhanVien = new SelectList(nhanvienList, "Value", "Text", selectedMaNhanVien?.ToString());
        }

        // Add this helper near the other private helpers (e.g. below PopulateSelectsAsync)
        private async Task LoadDropdowns(int? selectedNguoiId = null, int? selectedMaNhanVien = null)
        {
            // Reuse existing PopulateSelectsAsync for người mượn / nhân viên
            await PopulateSelectsAsync(selectedNguoiId, selectedMaNhanVien);

            // Load available copies for the view (same as used in Details)
            var cuonSachCoSan = await _context.CuonSachs
                .AsNoTracking()
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .Include(c => c.Sach)
                .ToListAsync();

            ViewBag.CuonSachCoSan = cuonSachCoSan;
        }

        // Function In SQL 
        public async Task<int> GetSoSachDangMuon(int MaNguoiMuon)
        {
            var result = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_TinhSoNgayTre({MaNguoiMuon})")
                .FirstOrDefaultAsync();

            return result;
        }

        // POST: PhieuMuons/TraSach - Cập nhật tình trạng trả sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraSach(int maPhieuMuon, int maCuon, DateTime ngayTra, ReturnCondition tinhTrangTra)
        {
            var chiTiet = await _context.ChiTietPhieuMuons
                .FirstOrDefaultAsync(ct => ct.MaPhieuMuon == maPhieuMuon && ct.MaCuon == maCuon);

            if (chiTiet == null) return NotFound();

            chiTiet.NgayTra = ngayTra;
            chiTiet.TinhTrangTra = tinhTrangTra;

            // Cập nhật trạng thái cuốn sách
            var cuonSach = await _context.CuonSachs.FindAsync(maCuon);
            if (cuonSach != null)
            {
                if (tinhTrangTra == ReturnCondition.Mat)
                {
                    cuonSach.TrangThai = CopyStatus.BaoTri;
                    cuonSach.TinhTrang = BookCondition.Mat;
                }
                else if (tinhTrangTra == ReturnCondition.Hong)
                {
                    cuonSach.TrangThai = CopyStatus.BaoTri;
                    cuonSach.TinhTrang = BookCondition.Hong;
                }
                else
                {
                    cuonSach.TrangThai = CopyStatus.CoSan;
                }
            }

            // Nếu tất cả sách đã trả -> cập nhật trạng thái phiếu mượn
            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == maPhieuMuon);

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

            return RedirectToAction(nameof(Edit), new { id = maPhieuMuon });
        }
    }
}
