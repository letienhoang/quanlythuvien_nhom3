using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Controllers
{
    public class PhieuMuonsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILibraryCodeGenerator _codeGen;

        public PhieuMuonsController(LibraryDbContext context, ILibraryCodeGenerator codeGen)
        {
            _context = context;
            _codeGen = codeGen;
        }

        // GET: PhieuMuons
        public async Task<IActionResult> Index()
        {
            var items = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .AsNoTracking()
                .ToListAsync();

            return View(items);
        }

        // GET: PhieuMuons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuMuon == null) return NotFound();

            return View(phieuMuon);
        }

        // GET: PhieuMuons/Create
        public async Task<IActionResult> Create()
        {
            var model = new PhieuMuon
            {
                MaPhieuMuon = await _codeGen.GenerateNextAsync<PhieuMuon>(p => p.MaPhieuMuon, "PM", 4),
                NgayMuon = DateTime.Now,
                HanTra = DateTime.Now.AddDays(14)
            };

            await PopulateSelectsAsync();
            ViewBag.GeneratedMaPhieuMuon = model.MaPhieuMuon;
            return View(model);
        }

        // POST: PhieuMuons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuMuon,NguoiMuonId,NhanVienId,NgayMuon,HanTra,TrangThai,SoNgayTre")] PhieuMuon phieuMuon)
        {
            // ensure unique code with retries
            phieuMuon.MaPhieuMuon = await _codeGen.GenerateNextWithRetriesAsync<PhieuMuon>(p => p.MaPhieuMuon, "PM", 4);

            if (ModelState.IsValid)
            {
                _context.Add(phieuMuon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectsAsync();
            ViewBag.GeneratedMaPhieuMuon = phieuMuon.MaPhieuMuon;
            return View(phieuMuon);
        }

        // GET: PhieuMuons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons.FindAsync(id);
            if (phieuMuon == null) return NotFound();

            await PopulateSelectsAsync(phieuMuon.NguoiMuonId, phieuMuon.NhanVienId);
            return View(phieuMuon);
        }

        // POST: PhieuMuons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaPhieuMuon,NguoiMuonId,NhanVienId,NgayMuon,HanTra,TrangThai,SoNgayTre")] PhieuMuon phieuMuon)
        {
            if (id != phieuMuon.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuMuon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuMuonExists(phieuMuon.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectsAsync(phieuMuon.NguoiMuonId, phieuMuon.NhanVienId);
            return View(phieuMuon);
        }

        // GET: PhieuMuons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.NguoiMuon)
                .Include(p => p.NhanVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuMuon == null) return NotFound();

            return View(phieuMuon);
        }

        // POST: PhieuMuons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuMuon = await _context.PhieuMuons.FindAsync(id);
            if (phieuMuon != null)
            {
                _context.PhieuMuons.Remove(phieuMuon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuMuonExists(int id) =>
            _context.PhieuMuons.Any(e => e.Id == id);

        private async Task PopulateSelectsAsync(int? selectedNguoiId = null, int? selectedNhanVienId = null)
        {
            var nguoiList = await _context.NguoiMuons
                .AsNoTracking()
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(n.HoTen) ? n.MaNguoiMuon : $"{n.HoTen} ({n.MaNguoiMuon})"
                })
                .ToListAsync();

            var nhanvienList = await _context.NhanViens
                .AsNoTracking()
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(n.HoTen) ? n.MaNhanVien : $"{n.HoTen} ({n.MaNhanVien})"
                })
                .ToListAsync();

            ViewBag.NguoiMuonId = new SelectList(nguoiList, "Value", "Text", selectedNguoiId?.ToString());
            ViewBag.NhanVienId = new SelectList(nhanvienList, "Value", "Text", selectedNhanVienId?.ToString());
        }
    }
}
