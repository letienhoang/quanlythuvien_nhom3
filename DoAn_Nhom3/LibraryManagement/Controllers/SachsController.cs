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
    public class SachsController : Controller
    {
        private readonly LibraryDbContext _context;

        public SachsController(LibraryDbContext context)
        {
            _context = context;
        }

        private void PopulateTacGiaDropDown(object selectedTacGia = null)
        {
            var list = _context.TacGias
                        .OrderBy(t => t.TenTacGia)
                        .Select(t => new { t.MaTacGia, t.TenTacGia })
                        .ToList();
            ViewBag.MaTacGia = new SelectList(list, "MaTacGia", "TenTacGia", selectedTacGia);
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
        public async Task<IActionResult> Details(string id)
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sachs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSach,TenSach,ISBN,NamXuatBan,NhaXuatBan,NgonNgu,SoTrang,MoTa,MaTacGia,SoLuong")] Sach sach)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateTacGiaDropDown(sach.TacGiaId);
            return View(sach);
        }

        // GET: Sachs/Edit/5
        public async Task<IActionResult> Edit(string id)
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
            return View(sach);
        }

        // POST: Sachs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSach,TenSach,ISBN,NamXuatBan,NhaXuatBan,NgonNgu,SoTrang,MoTa,MaTacGia,SoLuong")] Sach sach)
        {
            if (id != sach.MaSach)
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
            PopulateTacGiaDropDown(sach.TacGiaId);
            return View(sach);
        }

        // GET: Sachs/Delete/5
        public async Task<IActionResult> Delete(string id)
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sach = await _context.Sachs.FindAsync(id);
            if (sach != null)
            {
                _context.Sachs.Remove(sach);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SachExists(string id)
        {
            return _context.Sachs.Any(e => e.MaSach == id);
        }
    }
}
