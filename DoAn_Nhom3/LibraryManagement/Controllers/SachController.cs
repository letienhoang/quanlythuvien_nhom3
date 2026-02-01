using System.Data;
using LibraryManagement.DtosModels;
using LibraryManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagement.Controllers
{
    public class SachController : Controller
    {
        private readonly LibraryDbContext _context;

        public SachController(LibraryDbContext context)
        {
            _context = context;
        }

        private void PopulateTacGiaDropDown(object? selectedTacGia = null)
        {
            var list = _context.TacGia
                        .OrderBy(t => t.TenTacGia)
                        .Select(t => new { t.MaTacGia, t.TenTacGia })
                        .ToList();
            ViewBag.MaTacGia = new SelectList(list, "MaTacGia", "TenTacGia", selectedTacGia);
        }
        
        private async Task PopulateCategoriesFor(SachEditViewModel vm)
        {
            var cats = await _context.DanhMuc
                .AsNoTracking()
                .OrderBy(d => d.TenDanhMuc)
                .Select(d => new { d.MaDanhMuc, d.TenDanhMuc })
                .ToListAsync();

            vm.AvailableCategories = cats.Select(c => new SelectListItem
            {
                Value = c.MaDanhMuc.ToString(),
                Text = c.TenDanhMuc
            }).ToList();
        }
        
        private bool SachExists(int id)
        {
            return _context.Sach.Any(e => e.MaSach == id);
        }

        // GET: Sach
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Sach
                .AsNoTracking()
                .Include(c => c.TacGia);

            var total = await query.CountAsync();
            
            var totalPages = (int)Math.Ceiling((double)total / pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .OrderByDescending(c => c.MaSach)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Sach>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = total
            };
            return View(result);
        }

        // GET: Sach/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sach = await _context.Sach
                .AsNoTracking()
                .Include(s => s.TacGia) // optional
                .FirstOrDefaultAsync(m => m.MaSach == id.Value);

            if (sach == null) return NotFound();

            // lấy tên danh mục bằng query set-based
            var categoryNames = await _context.PhanLoai
                .AsNoTracking()
                .Where(pl => pl.MaSach == id.Value)
                .Join(_context.DanhMuc,
                    pl => pl.MaDanhMuc,
                    d => d.MaDanhMuc,
                    (pl, d) => new { d.MaDanhMuc, d.TenDanhMuc })
                .ToListAsync();

            ViewBag.Categories = categoryNames; 
            ViewBag.SoLuongSachCoSan = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_CountAvailableCopies({id.Value}) AS Value")
                .FirstOrDefaultAsync();

            return View(sach);
        }

        // GET: Sach/Create
        public async Task<IActionResult> Create()
        {
            var vm = new SachEditViewModel();
            PopulateTacGiaDropDown();
            await PopulateCategoriesFor(vm);
            return View(vm);
        }

        // POST: Sach/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SachEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateTacGiaDropDown(vm.MaTacGia);
                await PopulateCategoriesFor(vm);
                return View(vm);
            }
            
            await using var efTx = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            bool openedHere = false;

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                    openedHere = true;
                }

                await using var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "usp_InsertBookAndCopies";

                // Tạo parameter helper
                System.Data.Common.DbParameter MakeParam(string name, object? value, DbType? dbType = null)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = name;
                    p.Value = value ?? DBNull.Value;
                    if (dbType.HasValue) p.DbType = dbType.Value;
                    return p;
                }

                cmd.Parameters.Add(MakeParam("@TenSach", vm.TenSach));
                cmd.Parameters.Add(MakeParam("@ISBN", vm.ISBN));
                cmd.Parameters.Add(MakeParam("@NamXuatBan", vm.NamXuatBan, DbType.Int32));
                cmd.Parameters.Add(MakeParam("@NhaXuatBan", vm.NhaXuatBan));
                cmd.Parameters.Add(MakeParam("@NgonNgu", vm.NgonNgu));
                cmd.Parameters.Add(MakeParam("@SoTrang", vm.SoTrang, DbType.Int32));
                cmd.Parameters.Add(MakeParam("@MoTa", vm.MoTa));
                cmd.Parameters.Add(MakeParam("@MaTacGia", vm.MaTacGia, DbType.Int32));
                cmd.Parameters.Add(MakeParam("@SoLuong", vm.SoLuong ?? 0, DbType.Int32));
                
                var tvp = ConvertHelper.BuildIntListTvp(vm.SelectedCategoryIds);
                
                if (cmd is SqlCommand sqlCmd)
                {
                    var tvpParam = new SqlParameter("@MaDanhMucs", SqlDbType.Structured)
                    {
                        TypeName = "dbo.IntList",
                        Value = tvp
                    };
                    sqlCmd.Parameters.Add(tvpParam);
                }
                else
                {
                    throw new InvalidOperationException("Connection is not a SQL Server connection.");
                }

                // Gắn transaction hiện thời (EF transaction) vào command để command nằm trong cùng transaction
                if (efTx.GetDbTransaction() is System.Data.Common.DbTransaction nativeTx)
                {
                    cmd.Transaction = nativeTx;
                }

                // ExecuteScalarAsync vì stored proc trả SELECT @MaSach AS MaSach
                var scalar = await cmd.ExecuteScalarAsync();
                if (scalar == null || scalar == DBNull.Value)
                    throw new Exception("Stored procedure không trả về Mã Sách.");

                var newMaSach = Convert.ToInt32(scalar);

                await efTx.CommitAsync();
                TempData["Success"] = $"Đã thêm sách '{vm.TenSach}' (ID {newMaSach}).";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                try { await efTx.RollbackAsync(); } catch { }
                ModelState.AddModelError("", "Không thể lưu sách: " + ex.Message);
                TempData["Error"] = "Lỗi khi thêm sách: " + ex.Message;
                PopulateTacGiaDropDown(vm.MaTacGia);
                await PopulateCategoriesFor(vm);
                return View(vm);
            }
            finally
            {
                // Nếu chính ta mở connection thì đóng ở đây.
                if (openedHere)
                {
                    try { await conn.CloseAsync(); } catch { }
                }
            }
        }

        // GET: Sach/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sach = await _context.Sach
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSach == id.Value);

            if (sach == null) return NotFound();

            var vm = new SachEditViewModel
            {
                MaSach = sach.MaSach,
                TenSach = sach.TenSach,
                ISBN = sach.ISBN,
                NamXuatBan = sach.NamXuatBan,
                NhaXuatBan = sach.NhaXuatBan,
                NgonNgu = sach.NgonNgu,
                SoTrang = sach.SoTrang,
                MoTa = sach.MoTa,
                MaTacGia = sach.MaTacGia,
                SoLuong = sach.SoLuong
            };

            // load selected categories
            vm.SelectedCategoryIds = await _context.PhanLoai
                .Where(p => p.MaSach == id.Value)
                .Select(p => p.MaDanhMuc)
                .ToListAsync();

            PopulateTacGiaDropDown(vm.MaTacGia);
            await PopulateCategoriesFor(vm);

            return View(vm);
        }

        // POST: Sach/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maSach, SachEditViewModel vm)
        {
            if (maSach != vm.MaSach) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateTacGiaDropDown(vm.MaTacGia);
                await PopulateCategoriesFor(vm);
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Sach.FindAsync(maSach);
                if (existing == null) return NotFound();

                // update properties
                existing.TenSach = vm.TenSach;
                existing.ISBN = vm.ISBN;
                existing.NamXuatBan = vm.NamXuatBan;
                existing.NhaXuatBan = vm.NhaXuatBan;
                existing.NgonNgu = vm.NgonNgu;
                existing.SoTrang = vm.SoTrang;
                existing.MoTa = vm.MoTa;
                existing.MaTacGia = vm.MaTacGia;
                existing.SoLuong = vm.SoLuong;

                await _context.SaveChangesAsync();

                // sync PhanLoai
                var currentIds = await _context.PhanLoai
                    .Where(p => p.MaSach == maSach)
                    .Select(p => p.MaDanhMuc)
                    .ToListAsync();

                var newIds = (vm.SelectedCategoryIds ?? new List<int>()).Distinct().ToList();

                var toAdd = newIds.Except(currentIds).ToList();
                var toRemove = currentIds.Except(newIds).ToList();

                if (toAdd.Any())
                {
                    var adds = toAdd.Select(cid => new PhanLoai { MaSach = maSach, MaDanhMuc = cid });
                    _context.PhanLoai.AddRange(adds);
                }

                if (toRemove.Any())
                {
                    var removes = await _context.PhanLoai
                        .Where(p => p.MaSach == maSach && toRemove.Contains(p.MaDanhMuc))
                        .ToListAsync();
                    _context.PhanLoai.RemoveRange(removes);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                if (!SachExists(vm.MaSach)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Không thể lưu: " + ex.Message);
                PopulateTacGiaDropDown(vm.MaTacGia);
                await PopulateCategoriesFor(vm);
                return View(vm);
            }
        }

        // GET: Sach/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sach
                .FirstOrDefaultAsync(m => m.MaSach == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // POST: Sach/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Sach.FindAsync(id);
            if (sach != null)
            {
                var phans = _context.PhanLoai.Where(p => p.MaSach == id);
                _context.PhanLoai.RemoveRange(phans);

                _context.Sach.Remove(sach);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecalculateBookQuantities()
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
                cmd.CommandText = "sp_RecalculateBookQuantities_Cursor";
                
                await cmd.ExecuteNonQueryAsync();
        
                TempData["Success"] = $"Đã cập nhật số lượng sách có sẵn thành công.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật số lượng sách: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật số lượng sách: {ex.Message}";
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
