using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using Microsoft.Data.SqlClient;

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
                .FirstOrDefaultAsync(m => m.MaPhat == id);

            if (phieuPhat == null) return NotFound();

            return View(phieuPhat);
        }

        // GET: PhieuPhats/Create
        public async Task<IActionResult> Create()
        {
            var model = new PhieuPhat { };
            
            return View(model);
        }

        // POST: PhieuPhats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhat,MaPhieuMuon,SoTienPhat,LyDo,TrangThaiThanhToan")] PhieuPhat phieuPhat)
        {
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

            return View(phieuPhat);
        }

        // GET: PhieuPhats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phieuPhat = await _context.PhieuPhats.FindAsync(id);
            if (phieuPhat == null) return NotFound();

           return View(phieuPhat);
        }

        // POST: PhieuPhats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maPhat, [Bind("MaPhieuMuon,SoTienPhat,LyDo,TrangThaiThanhToan")] PhieuPhat phieuPhat)
        {
            if (!PhieuPhatExists(maPhat)) return NotFound();

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
                    if (!_context.PhieuPhats.Any(e => e.MaPhat == maPhat)) return NotFound();
                    throw;
                }
            }

            return View(phieuPhat);
        }

        // GET: PhieuPhats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phieuPhat = await _context.PhieuPhats
                .Include(p => p.PhieuMuon)
                .ThenInclude(pm => pm.NguoiMuon)
                .FirstOrDefaultAsync(m => m.MaPhat == id);

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
            return _context.PhieuPhats.Any(e => e.MaPhat == id);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUnpaidFines()
        {
            var conn = _context.Database.GetDbConnection();
            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                    openedHere = true;
                }
        
                if (conn is not SqlConnection sqlConn)
                    throw new InvalidOperationException("Kết nối DB không phải SqlConnection. Stored procedure chỉ hoạt động trên SQL Server.");
        
                await using var cmd = sqlConn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_UpdateUnpaidFines";
                
                await cmd.ExecuteNonQueryAsync();
        
                TempData["Success"] = $"Đã cập nhật tiền phạt cho các phiếu mượn quá hạn.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = $"Lỗi khi câp nhật tiền phạt: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật tiền phạt: {ex.Message}";
            }
            finally
            {
                if (openedHere)
                {
                    try { await conn.CloseAsync(); } catch { }
                }
            }
        
            return RedirectToAction(nameof(Index));
        }
    }
}
