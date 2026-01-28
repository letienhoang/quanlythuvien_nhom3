using LibraryManagement.DtosModels;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

public class ThongKeSachsController : Controller
{
    private readonly LibraryDbContext _context;

    public ThongKeSachsController(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.Database
            .SqlQuery<SachThongKeDto>($"EXEC sp_GetDanhSachSach")
            .ToListAsync();

        return View(data);
    }
}
