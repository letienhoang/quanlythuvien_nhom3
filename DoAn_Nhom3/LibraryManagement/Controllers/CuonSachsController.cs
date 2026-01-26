using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class CuonSachsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public CuonSachsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }

        private void PopulateSachDropDown(object? selectedSach = null)
        {
            var list = _context.Sachs
                        .OrderBy(s => s.TenSach)
                        .Select(s => new { s.Id, s.TenSach })
                        .ToList();

            ViewBag.SachId = new SelectList(list, "Id", "TenSach", selectedSach);
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
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSachs
                                .Include(c => c.Sach)
                                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuonSach == null) return NotFound();

            return View(cuonSach);
        }

        // GET: CuonSachs/Create
        public async Task<IActionResult> Create()
        {
            // generate suggestion for MaCuon
            var generated = await _codeGen.GenerateNextAsync<CuonSach>(c => c.MaCuon, "C", 6);
            var model = new CuonSach { MaCuon = generated, NgayNhap = DateTime.Now };
            PopulateSachDropDown();
            ViewBag.GeneratedMaCuon = generated;
            return View(model);
        }

        // POST: CuonSachs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCuon,SachId,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            // ensure unique/generated MaCuon (with retries)
            cuonSach.MaCuon = await _codeGen.GenerateNextWithRetriesAsync<CuonSach>(c => c.MaCuon, "C", 6);

            // server-side: ensure SachId exists
            if (!_context.Sachs.Any(s => s.Id == cuonSach.SachId))
            {
                ModelState.AddModelError(nameof(cuonSach.SachId), "Sách không hợp lệ. Vui lòng chọn sách từ danh sách.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(cuonSach);
                await _context.SaveChangesAsync();

                // Gọi SP đồng bộ số lượng sách
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_RecalculateBookQuantities_Cursor");

                return RedirectToAction(nameof(Index));
            }

            // repopulate dropdown and keep suggestion
            PopulateSachDropDown(cuonSach.SachId);
            ViewBag.GeneratedMaCuon = cuonSach.MaCuon;
            return View(cuonSach);
        }

        // GET: CuonSachs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSachs.FindAsync(id);
            if (cuonSach == null) return NotFound();

            PopulateSachDropDown(cuonSach.SachId);
            ViewBag.GeneratedMaCuon = cuonSach.MaCuon;
            return View(cuonSach);
        }

        // POST: CuonSachs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaCuon,SachId,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            if (id != cuonSach.Id) return NotFound();

            if (!_context.Sachs.Any(s => s.Id == cuonSach.SachId))
            {
                ModelState.AddModelError(nameof(cuonSach.SachId), "Sách không hợp lệ. Vui lòng chọn sách từ danh sách.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuonSach);
                    await _context.SaveChangesAsync();

                    // Gọi SP đồng bộ số lượng sách
                    await _context.Database.ExecuteSqlRawAsync("EXEC sp_RecalculateBookQuantities_Cursor");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuonSachExists(cuonSach.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSachDropDown(cuonSach.SachId);
            ViewBag.GeneratedMaCuon = cuonSach.MaCuon;
            return View(cuonSach);
        }

        // GET: CuonSachs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cuonSach = await _context.CuonSachs
                                .Include(c => c.Sach)
                                .FirstOrDefaultAsync(m => m.Id == id);
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

                // Gọi SP đồng bộ số lượng sách
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_RecalculateBookQuantities_Cursor");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CuonSachExists(int id)
        {
            return _context.CuonSachs.Any(e => e.Id == id);
        }
    }
}
