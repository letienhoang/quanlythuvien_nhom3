using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class CuonSachsController : Controller
    {
        private readonly LibraryDbContext _context;

        public CuonSachsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: CuonSachs
        public async Task<IActionResult> Index()
        {
            return View(await _context.CuonSachs.ToListAsync());
        }

        // GET: CuonSachs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuonSach = await _context.CuonSachs
                .FirstOrDefaultAsync(m => m.MaCuon == id);
            if (cuonSach == null)
            {
                return NotFound();
            }

            return View(cuonSach);
        }

        // GET: CuonSachs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CuonSachs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCuon,SachId,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cuonSach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cuonSach);
        }

        // GET: CuonSachs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuonSach = await _context.CuonSachs.FindAsync(id);
            if (cuonSach == null)
            {
                return NotFound();
            }
            return View(cuonSach);
        }

        // POST: CuonSachs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaCuon,SachId,TinhTrang,TrangThai,ViTriKe,NgayNhap")] CuonSach cuonSach)
        {
            if (id != cuonSach.MaCuon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuonSach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuonSachExists(cuonSach.MaCuon))
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
            return View(cuonSach);
        }

        // GET: CuonSachs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuonSach = await _context.CuonSachs
                .FirstOrDefaultAsync(m => m.MaCuon == id);
            if (cuonSach == null)
            {
                return NotFound();
            }

            return View(cuonSach);
        }

        // POST: CuonSachs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var cuonSach = await _context.CuonSachs.FindAsync(id);
            if (cuonSach != null)
            {
                _context.CuonSachs.Remove(cuonSach);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuonSachExists(string id)
        {
            return _context.CuonSachs.Any(e => e.MaCuon == id);
        }
    }
}
