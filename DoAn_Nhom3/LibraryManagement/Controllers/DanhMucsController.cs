using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Controllers
{
    public class DanhMucsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public DanhMucsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
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
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // GET: DanhMucs/Create
        public async Task<IActionResult> Create()
        {
            var generated = await _codeGen.GenerateNextAsync<DanhMuc>(d => d.MaDanhMuc, "DM", 4);
            var model = new DanhMuc { MaDanhMuc = generated };
            return View(model);
        }

        // POST: DanhMucs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDanhMuc, MoTa, MaDanhMuc")] DanhMuc danhMuc)
        {
            // generate server-side
            danhMuc.MaDanhMuc = await _codeGen.GenerateNextWithRetriesAsync<DanhMuc>(d => d.MaDanhMuc, "DM", 4);

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

            // show suggestion if fail
            var suggestion = await _codeGen.GenerateNextAsync<DanhMuc>(d => d.MaDanhMuc, "DM", 4);
            danhMuc.MaDanhMuc = suggestion;
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaDanhMuc,TenDanhMuc,MoTa")] DanhMuc danhMuc)
        {
            if (id != danhMuc.Id) return NotFound();

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
                    if (!DanhMucExists(danhMuc.Id)) return NotFound();
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
                .FirstOrDefaultAsync(m => m.Id == id.Value);

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
            return _context.DanhMucs.Any(e => e.Id == id);
        }
    }
}
