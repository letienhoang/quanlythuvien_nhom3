using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.ViewModels;

public class SachEditViewModel
{
    public int MaSach { get; set; }

    [Required, StringLength(250)]
    [Display(Name = "Tên sách")]
    public string TenSach { get; set; } = "";

    [Required, StringLength(50)]
    [Display(Name = "ISBN")]
    public string ISBN { get; set; } = "";

    [Range(0, 3000)]
    [Display(Name = "Năm xuất bản")]
    public int NamXuatBan { get; set; }

    [StringLength(200)]
    [Display(Name = "Nhà xuất bản")]
    public string? NhaXuatBan { get; set; }

    [StringLength(100)]
    [Display(Name = "Ngôn ngữ")]
    public string? NgonNgu { get; set; }

    [Display(Name = "Số trang")]
    public int? SoTrang { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required]
    [Display(Name = "Tác giả")]
    public int MaTacGia { get; set; }

    [Display(Name = "Số lượng")]
    public int? SoLuong { get; set; }

    // Categories selection
    public List<int> SelectedCategoryIds { get; set; } = new();

    // For UI
    public List<SelectListItem> AvailableCategories { get; set; } = new();
}