using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class TacGiaController : Controller
    {
        private readonly LibraryDbContext _context;

        public TacGiaController(LibraryDbContext context)
        {
            _context = context;
        }
        
        // GET: TacGia
        public async Task<IActionResult> Index()
        {
            var list = await _context.TacGia.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: TacGia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .Include(t => t.Sach)
                .FirstOrDefaultAsync(m => m.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // GET: TacGia/Create
        public async Task<IActionResult> Create()
        {
            var model = new TacGia { };
            return View(model);
        }

        // POST: TacGia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenTacGia,NgaySinh,QuocTich,MoTa")] TacGia tacGia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(tacGia);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) {
                    ModelState.AddModelError("", "Không thể lưu tác giả do xung đột mã; vui lòng thử lại.");
                }
            }
            
            return View(tacGia);
        }

        // GET: TacGia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia.FindAsync(id);
            if (tacGia == null)
            {
                return NotFound();
            }
            ViewBag.GeneratedMaTacGia = tacGia.MaTacGia;
            return View(tacGia);
        }

        // POST: TacGia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maTacGia, [Bind("TenTacGia,NgaySinh,QuocTich,MoTa")] TacGia tacGia)
        {
            if (maTacGia != tacGia.MaTacGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tacGia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TacGiaExists(tacGia.MaTacGia))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tacGia);
        }

        // GET: TacGia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .FirstOrDefaultAsync(m => m.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // POST: TacGia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tacGia = await _context.TacGia.FindAsync(id);
            if (tacGia != null)
            {
                _context.TacGia.Remove(tacGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TacGiaExists(int id)
        {
            return _context.TacGia.Any(e => e.MaTacGia == id);
        }
    }
}
