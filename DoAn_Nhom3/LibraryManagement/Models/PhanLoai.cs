using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhanLoai
{
    [Display(Name = "Sách")]
    public int MaSach { get; set; }

    [Display(Name = "Danh mục")]
    public int MaDanhMuc { get; set; }

    [ForeignKey(nameof(MaSach))]
    public Sach? Sach { get; set; }

    [ForeignKey(nameof(MaDanhMuc))]
    public DanhMuc? DanhMuc { get; set; }
}