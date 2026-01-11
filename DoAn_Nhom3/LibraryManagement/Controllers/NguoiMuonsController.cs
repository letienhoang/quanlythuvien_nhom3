using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class NguoiMuonsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public NguoiMuonsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }

        // GET: NguoiMuons
        public async Task<IActionResult> Index()
        {
            var list = await _context.NguoiMuons.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: NguoiMuons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nguoiMuon = await _context.NguoiMuons
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (nguoiMuon == null) return NotFound();

            return View(nguoiMuon);
        }

        // GET: NguoiMuons/Create
        public async Task<IActionResult> Create()
        {
            var generated = await _codeGen.GenerateNextAsync<NguoiMuon>(n => n.MaNguoiMuon, "NM", 4);
            var model = new NguoiMuon
            {
                MaNguoiMuon = generated,
                NgayDangKy = DateTime.Today,
                NgayHetHan = DateTime.Today.AddYears(1),
                LoaiDocGia = LibraryManagement.Enums.ReaderType.SinhVien, // mặc định, bạn đổi nếu muốn
                TrangThai = LibraryManagement.Enums.MemberStatus.HoatDong
            };
            return View(model);
        }

        // POST: NguoiMuons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNguoiMuon,HoTen,NgaySinh,CCCD,DiaChi,SoDienThoai,Email,LoaiDocGia,NgayDangKy,NgayHetHan,TrangThai")] NguoiMuon nguoiMuon)
        {
            // luôn generate server-side
            nguoiMuon.MaNguoiMuon = await _codeGen.GenerateNextWithRetriesAsync<NguoiMuon>(n => n.MaNguoiMuon, "NM", 4);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(nguoiMuon);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu người mượn do xung đột mã; vui lòng thử lại.");
                }
            }

            // nếu lỗi, gợi ý mã mới
            nguoiMuon.MaNguoiMuon = await _codeGen.GenerateNextAsync<NguoiMuon>(n => n.MaNguoiMuon, "NM", 4);
            return View(nguoiMuon);
        }

        // GET: NguoiMuons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nguoiMuon = await _context.NguoiMuons.FindAsync(id.Value);
            if (nguoiMuon == null) return NotFound();

            return View(nguoiMuon);
        }

        // POST: NguoiMuons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaNguoiMuon,HoTen,NgaySinh,CCCD,DiaChi,SoDienThoai,Email,LoaiDocGia,NgayDangKy,NgayHetHan,TrangThai")] NguoiMuon nguoiMuon)
        {
            if (id != nguoiMuon.Id) return NotFound();

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
                    if (!NguoiMuonExistsById(nguoiMuon.Id)) return NotFound();
                    throw;
                }
            }
            return View(nguoiMuon);
        }

        // GET: NguoiMuons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nguoiMuon = await _context.NguoiMuons
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (nguoiMuon == null) return NotFound();

            return View(nguoiMuon);
        }

        // POST: NguoiMuons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nguoiMuon = await _context.NguoiMuons.FindAsync(id);
            if (nguoiMuon != null)
            {
                _context.NguoiMuons.Remove(nguoiMuon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NguoiMuonExistsById(int id)
        {
            return _context.NguoiMuons.Any(e => e.Id == id);
        }
    }
}
