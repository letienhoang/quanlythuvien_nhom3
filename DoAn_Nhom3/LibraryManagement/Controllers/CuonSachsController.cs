using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class CuonSachsController : Controller
    {
        private readonly LibraryDbContext _context;

        public CuonSachsController(LibraryDbContext context)
        {
            _context = context;
        }

        private void PopulateSachDropDown(object? selectedSach = null)
        {
            var list = _context.Sachs
                        .OrderBy(s => s.TenSach)
                        .Select(s => new { s.MaSach, s.TenSach })
                        .ToList();

            ViewBag.MaSach = new SelectList(list, "MaSach", "TenSach", selectedSach);
        }

        // GET: CuonSachs
        public async Task<IActionResult> Index()
        {
            var cuonSachs = await _context.CuonSachs
                                .Include(c => c.Sach)
                                .ToListAsync();
            return View(cuonSachs);
        }

        // GET: CuonSachs/Details/5
        public async Task<IActionResult> Details(int? maSach)
        {
            if (maSach == null) return NotFound();

            var cuonSach = await _context.CuonSachs
                                .Include(c => c.Sach)
                                .FirstOrDefaultAsync(m => m.MaSach == maSach);
            if (cuonSach == null) return NotFound();

            return View(cuonSach);
        }

        // GET: CuonSachs/Create
        public async Task<IActionResult> Create()
        {
            // generate suggestion for MaCuon
            var model = new CuonSach { NgayNhap = DateTime.Now };
            PopulateSachDropDown();
            return View(model);
        }

        // POST: CuonSachs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSach,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            // server-side: ensure MaSach exists
            if (!_context.Sachs.Any(s => s.MaSach == cuonSach.MaSach))
            {
                ModelState.AddModelError(nameof(cuonSach.MaSach), "Sách không hợp lệ. Vui lòng chọn sách từ danh sách.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(cuonSach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // repopulate dropdown and keep suggestion
            PopulateSachDropDown(cuonSach.MaSach);
            return View(cuonSach);
        }

        // GET: CuonSachs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSachs.FindAsync(id);
            if (cuonSach == null) return NotFound();

            PopulateSachDropDown(cuonSach.MaSach);
            return View(cuonSach);
        }

        // POST: CuonSachs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maCuon, [Bind("MaSach,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            if (maCuon != cuonSach.MaCuon) return NotFound();

            if (!_context.Sachs.Any(s => s.MaSach == cuonSach.MaSach))
            {
                ModelState.AddModelError(nameof(cuonSach.MaSach), "Sách không hợp lệ. Vui lòng chọn sách từ danh sách.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuonSach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuonSachExists(cuonSach.MaCuon)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSachDropDown(cuonSach.MaSach);
            return View(cuonSach);
        }

        // GET: CuonSachs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSachs
                                .Include(c => c.Sach)
                                .FirstOrDefaultAsync(m => m.MaCuon == id);
            if (cuonSach == null) return NotFound();

            return View(cuonSach);
        }

        // POST: CuonSachs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuonSach = await _context.CuonSachs.FindAsync(id);
            if (cuonSach != null)
            {
                _context.CuonSachs.Remove(cuonSach);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CuonSachExists(int id)
        {
            return _context.CuonSachs.Any(e => e.MaCuon == id);
        }
    }
}
