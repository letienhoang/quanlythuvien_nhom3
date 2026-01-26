using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class SachsController : Controller
    {
        private readonly LibraryDbContext _context;

        public SachsController(LibraryDbContext context)
        {
            _context = context;
        }

        private void PopulateTacGiaDropDown(object? selectedTacGia = null)
        {
            var list = _context.TacGias
                        .OrderBy(t => t.TenTacGia)
                        .Select(t => new { t.MaTacGia, t.TenTacGia })
                        .ToList();
            ViewBag.MaTacGia = new SelectList(list, "MaTacGia", "TenTacGia", selectedTacGia);
        }
        
        private bool SachExists(int id)
        {
            return _context.Sachs.Any(e => e.MaSach == id);
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
                .FirstOrDefaultAsync(m => m.MaSach == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // GET: Sachs/Create
        public async Task<IActionResult> Create()
        {
            var model = new Sach { };
            PopulateTacGiaDropDown();
            return View(model);
        }

        // POST: Sachs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsertBookDto dto)
        {
            if (ModelState.IsValid)
            {
                //Store procedure to insert book and its copies (*)
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_InsertBookAndCopies {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",
                dto.MaSach, dto.TenSach, dto.ISBN, dto.NamXuatBan,
                dto.NhaXuatBan ?? (object)DBNull.Value,
                dto.NgonNgu ?? (object)DBNull.Value,
                dto.SoTrang ?? (object)DBNull.Value,
                dto.MoTa ?? (object)DBNull.Value,
                dto.MaTacGia, dto.SoLuong);

                TempData["Success"] = $"Đã thêm sách '{dto.TenSach}' với {dto.SoLuong} cuốn.";
                return RedirectToAction(nameof(Index));
            }
 
            PopulateTacGiaDropDown(dto.MaTacGia);
            return View(dto);
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
            PopulateTacGiaDropDown(sach.MaTacGia);
            return View(sach);
        }

        // POST: Sachs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maSach, [Bind("TenSach,ISBN,NamXuatBan,NhaXuatBan,NgonNgu,SoTrang,MoTa,MaTacGia,SoLuong")] Sach sach)
        {
            if (maSach != sach.MaSach)
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
                    if (!SachExists(sach.MaSach))
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
            PopulateTacGiaDropDown(sach.MaTacGia);
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
                .FirstOrDefaultAsync(m => m.MaSach == id);
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
