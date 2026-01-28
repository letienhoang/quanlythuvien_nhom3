using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.ViewModels;

public class DanhMucEditViewModel
{
    public DanhMuc DanhMuc { get; set; } = new();

    // Book list for UI
    public List<SelectListItem> AvailableBooks { get; set; } = new();

    // Selected book ids from client
    public List<int> SelectedBookIds { get; set; } = new();
}