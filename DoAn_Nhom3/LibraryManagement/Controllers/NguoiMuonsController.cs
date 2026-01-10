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
    public class NguoiMuonsController : Controller
    {
        private readonly LibraryDbContext _context;

        public NguoiMuonsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: NguoiMuons
        public async Task<IActionResult> Index()
        {
            return View(await _context.NguoiMuon.ToListAsync());
        }

        // GET: NguoiMuons/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiMuon = await _context.NguoiMuon
                .FirstOrDefaultAsync(m => m.MaNguoiMuon == id);
            if (nguoiMuon == null)
            {
                return NotFound();
            }

            return View(nguoiMuon);
        }

        // GET: NguoiMuons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NguoiMuons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNguoiMuon,HoTen,NgaySinh,CCCD,DiaChi,SoDienThoai,Email,LoaiDocGia,NgayDangKy,NgayHetHan,TrangThai")] NguoiMuon nguoiMuon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nguoiMuon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nguoiMuon);
        }

        // GET: NguoiMuons/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiMuon = await _context.NguoiMuon.FindAsync(id);
            if (nguoiMuon == null)
            {
                return NotFound();
            }
            return View(nguoiMuon);
        }

        // POST: NguoiMuons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaNguoiMuon,HoTen,NgaySinh,CCCD,DiaChi,SoDienThoai,Email,LoaiDocGia,NgayDangKy,NgayHetHan,TrangThai")] NguoiMuon nguoiMuon)
        {
            if (id != nguoiMuon.MaNguoiMuon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nguoiMuon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NguoiMuonExists(nguoiMuon.MaNguoiMuon))
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
            return View(nguoiMuon);
        }

        // GET: NguoiMuons/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiMuon = await _context.NguoiMuon
                .FirstOrDefaultAsync(m => m.MaNguoiMuon == id);
            if (nguoiMuon == null)
            {
                return NotFound();
            }

            return View(nguoiMuon);
        }

        // POST: NguoiMuons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nguoiMuon = await _context.NguoiMuon.FindAsync(id);
            if (nguoiMuon != null)
            {
                _context.NguoiMuon.Remove(nguoiMuon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NguoiMuonExists(string id)
        {
            return _context.NguoiMuon.Any(e => e.MaNguoiMuon == id);
        }
    }
}
