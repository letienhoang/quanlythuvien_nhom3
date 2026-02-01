using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly LibraryDbContext _context;

        public NhanVienController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: NhanVien
        public async Task<IActionResult> Index()
        {
            return View(await _context.NhanVien.AsNoTracking().ToListAsync());
        }

        // GET: NhanVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // GET: NhanVien/Create
        public async Task<IActionResult> Create()
        {
            var model = new NhanVien{};
            return View(model);
        }

        // POST: NhanVien/Create
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

        // GET: NhanVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // POST: NhanVien/Edit/5
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

        // GET: NhanVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // POST: NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanVien.Remove(nhanVien);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
