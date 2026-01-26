using Humanizer;
using LibraryManagement.Enums;
using LibraryManagement.Extensions;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
                    //Funtion số sách đang mượn 
                    var soSach = await _context.Database
                        .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({item.NguoiMuonId}) AS Value")
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

            // Tính số sách đang mượn của độc giả (dùng hàm SQL) và số slot còn lại
            var soSachDangMuon = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({phieuMuon.NguoiMuonId}) AS Value")
                .FirstOrDefaultAsync();

            var remainingSlots = Math.Max(0, 3 - soSachDangMuon);
            ViewBag.RemainingSlots = remainingSlots;

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

            // Kiểm tra số slot còn lại (server-side) bằng hàm fn_SoSachDangMuon
            var soSachDangMuon = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({phieuMuon.NguoiMuonId}) AS Value")
                .FirstOrDefaultAsync();

            var remainingSlots = Math.Max(0, 3 - soSachDangMuon);
            if (remainingSlots <= 0)
            {
                TempData["Error"] = "Độc giả đã mượn tối đa 3 cuốn.";
                return RedirectToAction(nameof(Details), new { id = phieuMuonId });
            }

            if (cuonSachIds.Length > remainingSlots)
            {
                TempData["Error"] = $"Bạn chỉ có thể thêm tối đa {remainingSlots} cuốn nữa.";
                return RedirectToAction(nameof(Details), new { id = phieuMuonId });
            }

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
                    if (cuonSach == null || cuonSach.TrangThai != CopyStatus.CoSan)
                    {
                        TempData["Error"] = "Có cuốn sách không còn khả dụng.";
                        return RedirectToAction(nameof(Details), new { id = phieuMuonId });
                    }
                    // Trigger trong database đã tự động cập nhật trạng thái cuốn sách thành 'DangMuon'
                    //cuonSach.TrangThai = CopyStatus.DangMuon;
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
            var dto = new CreateBorrowRecordDto
            {
                MaPhieuMuon = await _codeGen.GenerateNextAsync<PhieuMuon>(p => p.MaPhieuMuon, "PM", 4),
                HanTra = DateTime.Now.AddDays(14)
            };

            ViewBag.NguoiMuonId = new SelectList(await _context.NguoiMuons.ToListAsync(), "Id", "HoTen");
            ViewBag.NhanVienId = new SelectList(await _context.NhanViens.ToListAsync(), "Id", "HoTen");

            // Lấy danh sách cuốn sách có sẵn và số lượng bản còn sẵn cho từng đầu sách
            var cuonSachCoSan = await _context.CuonSachs
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .Include(c => c.Sach)
                .ToListAsync();

            // Tạo dictionary: SachId -> số lượng bản còn sẵn
            var sachCounts = new Dictionary<int, int>();
            foreach (var sachId in cuonSachCoSan.Select(c => c.SachId).Distinct())
            {
                var soConSan = await _context.Database
                    .SqlQuery<int>($"SELECT dbo.fn_CountAvailableCopies({sachId}) AS Value")
                    .FirstOrDefaultAsync();
                sachCounts[sachId] = soConSan;
            }

            ViewBag.CuonSachCoSan = cuonSachCoSan;
            ViewBag.SachCounts = sachCounts;

            return View(dto);

        }

        // POST: PhieuMuons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBorrowRecordDto dto)
        {
        var cuonSachIds = dto.CuonSachIds;

        // Validate chọn sách
        if (cuonSachIds == null || !cuonSachIds.Any())
        {
            ModelState.AddModelError("", "Vui lòng chọn ít nhất một cuốn sách.");
        }
        else if (cuonSachIds.Count > 3)
        {
            ModelState.AddModelError("", "Chỉ được mượn tối đa 3 cuốn sách.");
        }

        // Validate số sách đang mượn
        if (ModelState.IsValid)
        {
            var soSachDangMuon = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({dto.NguoiMuonId}) AS Value")
                .FirstOrDefaultAsync();

            if (soSachDangMuon + cuonSachIds!.Count > 3)
            {
                ModelState.AddModelError(
                    "",
                    $"Độc giả đã mượn {soSachDangMuon} cuốn, chỉ được mượn tối đa 3 cuốn."
                );
            }
        }

        // Nếu có lỗi → trả view
        if (!ModelState.IsValid)
        {
            await LoadDropdowns(dto.NguoiMuonId, dto.NhanVienId);
            ViewBag.CuonSachCoSan = await _context.CuonSachs
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .Include(c => c.Sach)
                .ToListAsync();

            return View(dto);
        }

        // Tạo phiếu mượn
        var danhSachCuonSachId = string.Join(",", cuonSachIds!);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC usp_CreateBorrowRecord {0}, {1}, {2}, {3}, {4}",
            dto.MaPhieuMuon,
            dto.NguoiMuonId,
            dto.NhanVienId,
            danhSachCuonSachId,
            dto.HanTra
        );

        TempData["Success"] = $"Đã tạo phiếu mượn với {cuonSachIds!.Count} cuốn sách.";
        return RedirectToAction(nameof(Index));
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

        // Add this helper near the other private helpers (e.g. below PopulateSelectsAsync)
        private async Task LoadDropdowns(int? selectedNguoiId = null, int? selectedNhanVienId = null)
        {
            // Reuse existing PopulateSelectsAsync for người mượn / nhân viên
            await PopulateSelectsAsync(selectedNguoiId, selectedNhanVienId);

            // Load available copies for the view (same as used in Details)
            var cuonSachCoSan = await _context.CuonSachs
                .AsNoTracking()
                .Where(c => c.TrangThai == CopyStatus.CoSan)
                .Include(c => c.Sach)
                .ToListAsync();

            ViewBag.CuonSachCoSan = cuonSachCoSan;
        }

        // Function In SQL 
        public async Task<int> GetSoSachDangMuon(int nguoiMuonId)
        {
            var result = await _context.Database
               .SqlQuery<int>($"SELECT dbo.fn_SoSachDangMuon({nguoiMuonId})")
               .FirstOrDefaultAsync();

            return result;
        }

        // POST: PhieuMuons/TraSach - Cập nhật tình trạng trả sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraSach(int phieuMuonId, int cuonSachId, DateTime ngayTra, ReturnCondition tinhTrangTra)
        {
            // usp_ReturnBooks
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_ReturnBooks @p0, @p1, @p2, @p3",
                phieuMuonId,
                cuonSachId,
                (int)tinhTrangTra,
                ngayTra
            );
            TempData["Success"] = "Đã cập nhật tình trạng trả sách thành công.";
            return RedirectToAction(nameof(Edit), new { id = phieuMuonId });
 
        }

        [HttpPost]
        public IActionResult GiaHan(int phieuMuonId, int soNgayGiaHan)
        {
            // usp_RenewLoan
            _context.Database.ExecuteSqlRaw(
                "EXEC usp_RenewLoan @p0, @p1",
                phieuMuonId,
                soNgayGiaHan
            );

            TempData["Success"] = "Gia hạn phiếu mượn thành công";
            return RedirectToAction("Details", new { id = phieuMuonId });
        }
    }
}
