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
    public class PhieuPhatsController : Controller
    {
        private readonly LibraryDbContext _context;

        public PhieuPhatsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: PhieuPhats
        public async Task<IActionResult> Index()
        {
            return View(await _context.PhieuPhat.ToListAsync());
        }

        // GET: PhieuPhats/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuPhat = await _context.PhieuPhat
                .FirstOrDefaultAsync(m => m.MaPhat == id);
            if (phieuPhat == null)
            {
                return NotFound();
            }

            return View(phieuPhat);
        }

        // GET: PhieuPhats/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PhieuPhats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhat,MaPhieuMuon,SoTienPhat,LyDo,TrangThaiThanhToan")] PhieuPhat phieuPhat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuPhat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phieuPhat);
        }

        // GET: PhieuPhats/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuPhat = await _context.PhieuPhat.FindAsync(id);
            if (phieuPhat == null)
            {
                return NotFound();
            }
            return View(phieuPhat);
        }

        // POST: PhieuPhats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaPhat,MaPhieuMuon,SoTienPhat,LyDo,TrangThaiThanhToan")] PhieuPhat phieuPhat)
        {
            if (id != phieuPhat.MaPhat)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuPhat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuPhatExists(phieuPhat.MaPhat))
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
            return View(phieuPhat);
        }

        // GET: PhieuPhats/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuPhat = await _context.PhieuPhat
                .FirstOrDefaultAsync(m => m.MaPhat == id);
            if (phieuPhat == null)
            {
                return NotFound();
            }

            return View(phieuPhat);
        }

        // POST: PhieuPhats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phieuPhat = await _context.PhieuPhat.FindAsync(id);
            if (phieuPhat != null)
            {
                _context.PhieuPhat.Remove(phieuPhat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuPhatExists(string id)
        {
            return _context.PhieuPhat.Any(e => e.MaPhat == id);
        }
    }
}
