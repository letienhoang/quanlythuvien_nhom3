using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;

namespace LibraryManagement.Controllers
{
    public class CuonSachController : Controller
    {
        private readonly LibraryDbContext _context;

        public CuonSachController(LibraryDbContext context)
        {
            _context = context;
        }

        private void PopulateSachDropDown(object? selectedSach = null)
        {
            var list = _context.Sach
                        .OrderBy(s => s.TenSach)
                        .Select(s => new { s.MaSach, s.TenSach })
                        .ToList();

            ViewBag.MaSach = new SelectList(list, "MaSach", "TenSach", selectedSach);
        }

        // GET: CuonSach
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.CuonSach
                .AsNoTracking()
                .Include(c => c.Sach);

            var total = await query.CountAsync();
            
            var totalPages = (int)Math.Ceiling((double)total / pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .OrderByDescending(c => c.MaCuon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<CuonSach>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = total
            };

            return View(result);
        }

        // GET: CuonSach/Details/5
        public async Task<IActionResult> Details(int? maSach)
        {
            if (maSach == null) return NotFound();

            var cuonSach = await _context.CuonSach
                                .Include(c => c.Sach)
                                .FirstOrDefaultAsync(m => m.MaSach == maSach);
            if (cuonSach == null) return NotFound();

            return View(cuonSach);
        }

        // GET: CuonSach/Create
        public async Task<IActionResult> Create()
        {
            // generate suggestion for MaCuon
            var model = new CuonSach { NgayNhap = DateTime.Now };
            PopulateSachDropDown();
            return View(model);
        }

        // POST: CuonSach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSach,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            // server-side: ensure MaSach exists
            if (!_context.Sach.Any(s => s.MaSach == cuonSach.MaSach))
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

        // GET: CuonSach/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSach.FindAsync(id);
            if (cuonSach == null) return NotFound();

            PopulateSachDropDown(cuonSach.MaSach);
            return View(cuonSach);
        }

        // POST: CuonSach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maCuon, [Bind("MaSach,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            if (!CuonSachExists(maCuon)) return NotFound();

            if (!_context.Sach.Any(s => s.MaSach == cuonSach.MaSach))
            {
                ModelState.AddModelError(nameof(cuonSach.MaSach), "Sách không hợp lệ. Vui lòng chọn sách từ danh sách.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    cuonSach.MaCuon = maCuon;
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

        // GET: CuonSach/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSach
                                .Include(c => c.Sach)
                                .FirstOrDefaultAsync(m => m.MaCuon == id);
            if (cuonSach == null) return NotFound();

            return View(cuonSach);
        }

        // POST: CuonSach/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuonSach = await _context.CuonSach.FindAsync(id);
            if (cuonSach != null)
            {
                _context.CuonSach.Remove(cuonSach);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CuonSachExists(int id)
        {
            return _context.CuonSach.Any(e => e.MaCuon == id);
        }
    }
}
