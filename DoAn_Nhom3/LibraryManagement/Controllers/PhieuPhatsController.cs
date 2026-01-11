using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Controllers
{
    public class PhieuPhatsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public PhieuPhatsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }

        // GET: PhieuPhats
        public async Task<IActionResult> Index()
        {
            var list = await _context.PhieuPhats
                .Include(p => p.PhieuMuon)
                .ThenInclude(pm => pm.NguoiMuon)
                .ToListAsync();
            return View(list);
        }

        // GET: PhieuPhats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phieuPhat = await _context.PhieuPhats
                .Include(p => p.PhieuMuon)
                .ThenInclude(pm => pm.NguoiMuon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuPhat == null) return NotFound();

            return View(phieuPhat);
        }

        // GET: PhieuPhats/Create
        public async Task<IActionResult> Create()
        {
            var model = new PhieuPhat
            {
                MaPhat = await _codeGen.GenerateNextAsync<PhieuPhat>(p => p.MaPhat, "PP", 4)
            };
            ViewBag.PhieuMuonId = new SelectList(await _context.PhieuMuons
                                                .Include(pm => pm.NguoiMuon)
                                                .ToListAsync(), "Id", "MaPhieuMuon");
            return View(model);
        }

        // POST: PhieuPhats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhat,PhieuMuonId,SoTienPhat,LyDo,TrangThaiThanhToan")] PhieuPhat phieuPhat)
        {
            // Generate server-side (to avoid collisions / user spoofing)
            phieuPhat.MaPhat = await _codeGen.GenerateNextWithRetriesAsync<PhieuPhat>(p => p.MaPhat, "PP", 4);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(phieuPhat);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu phiếu phạt do xung đột mã. Vui lòng thử lại.");
                }
            }

            ViewBag.GeneratedMaPhat = phieuPhat.MaPhat;
            ViewBag.PhieuMuonId = new SelectList(await _context.PhieuMuons
                                                .Include(pm => pm.NguoiMuon)
                                                .ToListAsync(), "Id", "MaPhieuMuon", phieuPhat.PhieuMuonId);
            return View(phieuPhat);
        }

        // GET: PhieuPhats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phieuPhat = await _context.PhieuPhats.FindAsync(id);
            if (phieuPhat == null) return NotFound();

            ViewBag.PhieuMuonId = new SelectList(await _context.PhieuMuons
                                                .Include(pm => pm.NguoiMuon)
                                                .ToListAsync(), "Id", "MaPhieuMuon", phieuPhat.PhieuMuonId);
            return View(phieuPhat);
        }

        // POST: PhieuPhats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaPhat,PhieuMuonId,SoTienPhat,LyDo,TrangThaiThanhToan")] PhieuPhat phieuPhat)
        {
            if (id != phieuPhat.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuPhat);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PhieuPhats.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }

            ViewBag.PhieuMuonId = new SelectList(await _context.PhieuMuons
                                                .Include(pm => pm.NguoiMuon)
                                                .ToListAsync(), "Id", "MaPhieuMuon", phieuPhat.PhieuMuonId);
            return View(phieuPhat);
        }

        // GET: PhieuPhats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phieuPhat = await _context.PhieuPhats
                .Include(p => p.PhieuMuon)
                .ThenInclude(pm => pm.NguoiMuon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuPhat == null) return NotFound();

            return View(phieuPhat);
        }

        // POST: PhieuPhats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuPhat = await _context.PhieuPhats.FindAsync(id);
            if (phieuPhat != null)
            {
                _context.PhieuPhats.Remove(phieuPhat);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuPhatExists(int id)
        {
            return _context.PhieuPhats.Any(e => e.Id == id);
        }
    }
}
