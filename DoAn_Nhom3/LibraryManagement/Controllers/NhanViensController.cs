using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class NhanViensController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public NhanViensController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // GET: NhanViens/Create
        public async Task<IActionResult> Create()
        {
            var model = new NhanVien
            {
                MaNhanVien = await _codeGen.GenerateNextAsync<NhanVien>(x => x.MaNhanVien, "NV", 4)
            };
            return View(model);
        }

        // POST: NhanViens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "MaNhanVien,HoTen,NgaySinh,CCCD,ChucVu,SoDienThoai,Email,TaiKhoan,MatKhau")]
            NhanVien nhanVien)
        {
            nhanVien.MaNhanVien =
                await _codeGen.GenerateNextWithRetriesAsync<NhanVien>(x => x.MaNhanVien, "NV", 4);

            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            nhanVien.MaNhanVien =
                await _codeGen.GenerateNextAsync<NhanVien>(x => x.MaNhanVien, "NV", 4);

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
        public async Task<IActionResult> Edit(int id, [Bind(
            "Id,MaNhanVien,HoTen,NgaySinh,CCCD,ChucVu,SoDienThoai,Email,TaiKhoan,MatKhau")]
            NhanVien nhanVien)
        {
            if (id != nhanVien.Id) return NotFound();

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
                .FirstOrDefaultAsync(m => m.Id == id);

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
