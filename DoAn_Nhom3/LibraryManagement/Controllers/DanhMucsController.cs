using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class DanhMucsController : Controller
    {
        private readonly LibraryDbContext _context;

        public DanhMucsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: DanhMucs
        public async Task<IActionResult> Index()
        {
            var list = await _context.DanhMucs.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: DanhMucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id.Value);

            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // GET: DanhMucs/Create
        public async Task<IActionResult> Create()
        {
            var model = new DanhMuc { };
            return View(model);
        }

        // POST: DanhMucs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDanhMuc, MoTa")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(danhMuc);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu danh mục do xung đột mã; vui lòng thử lại.");
                }
            }
            
            return View(danhMuc);
        }

        // GET: DanhMucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs.FindAsync(id.Value);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: DanhMucs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maDanhMuc, [Bind("TenDanhMuc,MoTa")] DanhMuc danhMuc)
        {
            if (maDanhMuc != danhMuc.MaDanhMuc) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucExists(danhMuc.MaDanhMuc)) return NotFound();
                    throw;
                }
            }
            return View(danhMuc);
        }

        // GET: DanhMucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id.Value);

            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: DanhMucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
        }
    }
}
