using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class TacGiasController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public TacGiasController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }
        
        // GET: TacGias
        public async Task<IActionResult> Index()
        {
            var list = await _context.TacGias.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: TacGias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGias
                .Include(t => t.Sachs)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // GET: TacGias/Create
        public async Task<IActionResult> Create()
        {
            var generated = await _codeGen.GenerateNextAsync<TacGia>(t => t.MaTacGia, "TG", 4);
            var model = new TacGia { MaTacGia = generated };
            return View(model);
        }

        // POST: TacGias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTacGia,TenTacGia,NgaySinh,QuocTich,MoTa")] TacGia tacGia)
        {
            tacGia.MaTacGia = await _codeGen.GenerateNextWithRetriesAsync<TacGia>(t => t.MaTacGia, "TG", 4);
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

            var suggestion = await _codeGen.GenerateNextAsync<TacGia>(t => t.MaTacGia, "TG", 4);
            tacGia.MaTacGia = suggestion;
            return View(tacGia);
        }

        // GET: TacGias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGias.FindAsync(id);
            if (tacGia == null)
            {
                return NotFound();
            }
            ViewBag.GeneratedMaTacGia = tacGia.MaTacGia;
            return View(tacGia);
        }

        // POST: TacGias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaTacGia,TenTacGia,NgaySinh,QuocTich,MoTa")] TacGia tacGia)
        {
            if (id != tacGia.Id)
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
                    if (!TacGiaExists(tacGia.Id))
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

        // GET: TacGias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGias
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // POST: TacGias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tacGia = await _context.TacGias.FindAsync(id);
            if (tacGia != null)
            {
                _context.TacGias.Remove(tacGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TacGiaExists(int id)
        {
            return _context.TacGias.Any(e => e.Id == id);
        }
    }
}
