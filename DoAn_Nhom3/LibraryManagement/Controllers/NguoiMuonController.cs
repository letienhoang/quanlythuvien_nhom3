using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class NguoiMuonController : Controller
    {
        private readonly LibraryDbContext _context;

        public NguoiMuonController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: NguoiMuon
        public async Task<IActionResult> Index()
        {
            var list = await _context.NguoiMuon.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: NguoiMuon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nguoiMuon = await _context.NguoiMuon
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNguoiMuon == id.Value);

            if (nguoiMuon == null) return NotFound();

            return View(nguoiMuon);
        }

        // GET: NguoiMuon/Create
        public async Task<IActionResult> Create()
        {
            var model = new NguoiMuon
            {
                NgayDangKy = DateTime.Today,
                NgayHetHan = DateTime.Today.AddYears(1),
                LoaiDocGia = LibraryManagement.Enums.ReaderType.SinhVien, // mặc định, bạn đổi nếu muốn
                TrangThai = LibraryManagement.Enums.MemberStatus.HoatDong
            };
            return View(model);
        }

        // POST: NguoiMuon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,NgaySinh,CCCD,DiaChi,SoDienThoai,Email,LoaiDocGia,NgayDangKy,NgayHetHan,TrangThai")] NguoiMuon nguoiMuon)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(nguoiMuon);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu người mượn: " + ex.Message);
                    TempData["ErrorMessage"] = "Không thể lưu người mượn do xung đột dữ liệu. Vui lòng kiểm tra lại.";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi khi lưu người mượn: {ex.Message}");
                    TempData["ErrorMessage"] = "Không thể lưu người mượn do lỗi hệ thống. Vui lòng thử lại sau.";
                }
            }

            return View(nguoiMuon);
        }

        // GET: NguoiMuon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nguoiMuon = await _context.NguoiMuon.FindAsync(id.Value);
            if (nguoiMuon == null) return NotFound();

            return View(nguoiMuon);
        }

        // POST: NguoiMuon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maNguoiMuon, [Bind("HoTen,NgaySinh,CCCD,DiaChi,SoDienThoai,Email,LoaiDocGia,NgayDangKy,NgayHetHan,TrangThai")] NguoiMuon nguoiMuon)
        {
            if (maNguoiMuon != nguoiMuon.MaNguoiMuon) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nguoiMuon);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NguoiMuonExistsById(nguoiMuon.MaNguoiMuon)) return NotFound();
                    throw;
                }
            }
            return View(nguoiMuon);
        }

        // GET: NguoiMuon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nguoiMuon = await _context.NguoiMuon
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNguoiMuon == id.Value);

            if (nguoiMuon == null) return NotFound();

            return View(nguoiMuon);
        }

        // POST: NguoiMuon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nguoiMuon = await _context.NguoiMuon.FindAsync(id);
            if (nguoiMuon != null)
            {
                _context.NguoiMuon.Remove(nguoiMuon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NguoiMuonExistsById(int id)
        {
            return _context.NguoiMuon.Any(e => e.MaNguoiMuon == id);
        }
    }
}
