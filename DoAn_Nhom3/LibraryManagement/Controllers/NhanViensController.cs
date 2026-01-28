using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class NhanViensController : Controller
    {
        private readonly LibraryDbContext _context;

        public NhanViensController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: NhanViens
        public async Task<IActionResult> Index()
        {
            return View(await _context.NhanViens.AsNoTracking().ToListAsync());
        }

        // GET: NhanViens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // GET: NhanViens/Create
        public async Task<IActionResult> Create()
        {
            var model = new NhanVien{};
            return View(model);
        }

        // POST: NhanViens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "MaNhanVien,HoTen,NgaySinh,CCCD,ChucVu,SoDienThoai,Email,TaiKhoan,MatKhau")]
            NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(nhanVien);
        }

        // GET: NhanViens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // POST: NhanViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maNhanVien, [Bind(
            "HoTen,NgaySinh,CCCD,ChucVu,SoDienThoai,Email,TaiKhoan,MatKhau")]
            NhanVien nhanVien)
        {
            if (maNhanVien != nhanVien.MaNhanVien) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nhanVien);
        }

        // GET: NhanViens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // POST: NhanViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
