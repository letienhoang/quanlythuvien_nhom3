using LibraryManagement.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;

namespace LibraryManagement.Controllers
{
    public class DanhMucsController : Controller
    {
        private readonly LibraryDbContext _context;

        public DanhMucsController(LibraryDbContext context)
        {
            _context = context;
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

            // Load DanhMuc basic
            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id.Value);

            if (danhMuc == null) return NotFound();

            // Lấy sách liên quan qua PhanLoais -> Sach (set-based)
            var bookItems = await _context.PhanLoais
                .AsNoTracking()
                .Where(pl => pl.MaDanhMuc == id.Value)
                .Select(pl => new
                {
                    pl.MaSach,
                    TenSach = pl.Sach != null ? pl.Sach.TenSach : null,
                    ISBN = pl.Sach != null ? pl.Sach.ISBN : null,
                    TacGiaName = pl.Sach != null && pl.Sach.TacGia != null ? pl.Sach.TacGia.TenTacGia : null,
                    SoLuong = pl.Sach != null ? (pl.Sach.SoLuong ?? 0) : 0
                })
                .ToListAsync();

            var bookIds = bookItems.Select(b => b.MaSach).ToList();

            // Lấy số bản có sẵn (CoSan) nhóm theo MaSach
            var availableCounts = await _context.CuonSachs
                .AsNoTracking()
                .Where(c => bookIds.Contains(c.MaSach) && c.TrangThai == CopyStatus.CoSan)
                .GroupBy(c => c.MaSach)
                .Select(g => new { MaSach = g.Key, Count = g.Count() })
                .ToListAsync();

            var vm = new DanhMucDetailsViewModel
            {
                DanhMuc = danhMuc,
                Books = bookItems.Select(b => new DanhMucDetailsViewModel.BookListItem
                {
                    MaSach = b.MaSach,
                    TenSach = b.TenSach,
                    ISBN = b.ISBN,
                    TacGia = b.TacGiaName,
                    SoLuong = b.SoLuong,
                    AvailableCopies = availableCounts.FirstOrDefault(a => a.MaSach == b.MaSach)?.Count ?? 0
                }).ToList()
            };

            return View(vm);
        }

        // GET: DanhMucs/Create
        public async Task<IActionResult> Create()
        {
            var vm = new DanhMucEditViewModel();
            await PopulateBooksFor(vm);
            return View(vm);
        }

        // POST: DanhMucs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMucEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateBooksFor(vm);
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lưu DanhMuc trước để có MaDanhMuc (IDENTITY)
                _context.DanhMucs.Add(vm.DanhMuc);
                await _context.SaveChangesAsync();

                // Nếu có sách chọn -> tạo PhanLoai (nếu chưa tồn tại)
                if (vm.SelectedBookIds != null && vm.SelectedBookIds.Any())
                {
                    var distinctIds = vm.SelectedBookIds.Distinct().ToList();

                    var phanLoais = distinctIds.Select(sid => new PhanLoai
                    {
                        MaSach = sid,
                        MaDanhMuc = vm.DanhMuc.MaDanhMuc
                    }).ToList();

                    // tránh duplicate key (nếu có ràng buộc unique)
                    _context.AddRange(phanLoais);
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Không thể lưu danh mục: " + ex.Message);
                await PopulateBooksFor(vm);
                return View(vm);
            }
        }

        // GET: DanhMucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDanhMuc == id.Value);

            if (danhMuc == null) return NotFound();

            var vm = new DanhMucEditViewModel { DanhMuc = danhMuc };

            // load selected book ids from PhanLoais
            vm.SelectedBookIds = await _context.PhanLoais
                .Where(p => p.MaDanhMuc == danhMuc.MaDanhMuc)
                .Select(p => p.MaSach)
                .ToListAsync();

            await PopulateBooksFor(vm);
            return View(vm);
        }

        // POST: DanhMucs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMucEditViewModel vm)
        {
            if (id != vm.DanhMuc.MaDanhMuc) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateBooksFor(vm);
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load existing entity to update tracked entity (avoid attaching detached instance)
                var existing = await _context.DanhMucs.FindAsync(id);
                if (existing == null) return NotFound();

                // Update simple properties
                existing.TenDanhMuc = vm.DanhMuc.TenDanhMuc;
                existing.MoTa = vm.DanhMuc.MoTa;

                await _context.SaveChangesAsync();

                // Sync PhanLoais (set-based)
                var currentBookIds = await _context.PhanLoais
                    .Where(p => p.MaDanhMuc == id)
                    .Select(p => p.MaSach)
                    .ToListAsync();

                var newBookIds = (vm.SelectedBookIds ?? new List<int>()).Distinct().ToList();

                var toAdd = newBookIds.Except(currentBookIds).ToList();
                var toRemove = currentBookIds.Except(newBookIds).ToList();

                if (toAdd.Any())
                {
                    var addEntities = toAdd.Select(bid => new PhanLoai
                    {
                        MaSach = bid,
                        MaDanhMuc = id
                    });
                    _context.PhanLoais.AddRange(addEntities);
                }

                if (toRemove.Any())
                {
                    var removeEntities = await _context.PhanLoais
                        .Where(p => p.MaDanhMuc == id && toRemove.Contains(p.MaSach))
                        .ToListAsync();
                    _context.PhanLoais.RemoveRange(removeEntities);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                if (!DanhMucExists(vm.DanhMuc.MaDanhMuc))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Không thể lưu thay đổi: " + ex.Message);
                await PopulateBooksFor(vm);
                return View(vm);
            }
        }
        
        // GET: DanhMucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id.Value);

            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // Delete / Index etc remain mostly the same, but ensure cascade or manual cleanup of PhanLoais on delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                // Option A: if FK PhanLoais has cascade delete, direct remove ok
                // Option B: remove PhanLoais manually to be safe
                var phans = _context.PhanLoais.Where(p => p.MaDanhMuc == id);
                _context.PhanLoais.RemoveRange(phans);

                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
        }

        // Helper to populate book list
        private async Task PopulateBooksFor(DanhMucEditViewModel vm)
        {
            var books = await _context.Sachs
                .AsNoTracking()
                .OrderBy(b => b.TenSach)
                .Select(b => new { b.MaSach, b.TenSach, b.ISBN })
                .ToListAsync();

            vm.AvailableBooks = books.Select(b => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = b.MaSach.ToString(),
                Text = $"{b.TenSach} {(string.IsNullOrEmpty(b.ISBN) ? "" : $"({b.ISBN})")}",
                Selected = vm.SelectedBookIds?.Contains(b.MaSach) ?? false
            }).ToList();
        }
    }
}
