using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class SachsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public SachsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }

        private void PopulateTacGiaDropDown(object? selectedTacGia = null)
        {
            var list = _context.TacGias
                        .OrderBy(t => t.TenTacGia)
                        .Select(t => new { t.Id, t.TenTacGia })
                        .ToList();
            ViewBag.TacGiaId = new SelectList(list, "Id", "TenTacGia", selectedTacGia);
        }
        
        private bool SachExists(int id)
        {
            return _context.Sachs.Any(e => e.Id == id);
        }

        // GET: Sachs
        public async Task<IActionResult> Index()
        {
            var books = await _context.Sachs
                 .Include(s => s.TacGia)
                 .ToListAsync();
            return View(books);
        }

        // GET: Sachs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // GET: Sachs/Create
        public async Task<IActionResult> Create()
        {
            var generated = await _codeGen.GenerateNextAsync<Sach>(t => t.MaSach, "S", 6);
            var model = new Sach { MaSach = generated };
            PopulateTacGiaDropDown();
            return View(model);
        }

        // POST: Sachs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSach,TenSach,ISBN,NamXuatBan,NhaXuatBan,NgonNgu,SoTrang,MoTa,TacGiaId,SoLuong")] Sach sach)
        {
            sach.MaSach = await _codeGen.GenerateNextWithRetriesAsync<Sach>(t => t.MaSach, "S", 6);
            if (ModelState.IsValid)
            {
                _context.Add(sach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateTacGiaDropDown(sach.TacGiaId);
            var suggestion = await _codeGen.GenerateNextAsync<Sach>(t => t.MaSach, "S", 6);
            sach.MaSach = suggestion;
            return View(sach);
        }

        // GET: Sachs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null)
            {
                return NotFound();
            }
            PopulateTacGiaDropDown(sach.TacGiaId);
            ViewBag.GeneratedMaSach = sach.MaSach;
            return View(sach);
        }

        // POST: Sachs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSach,TenSach,ISBN,NamXuatBan,NhaXuatBan,NgonNgu,SoTrang,MoTa,TacGiaId,SoLuong")] Sach sach)
        {
            if (id != sach.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SachExists(sach.Id))
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
            PopulateTacGiaDropDown(sach.TacGiaId);
            return View(sach);
        }

        // GET: Sachs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // POST: Sachs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Sachs.FindAsync(id);
            if (sach != null)
            {
                _context.Sachs.Remove(sach);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
