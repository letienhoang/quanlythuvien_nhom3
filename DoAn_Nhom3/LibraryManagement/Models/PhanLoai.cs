using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhanLoai
{
    [Display(Name = "Sách")]
    public int SachId { get; set; }

    [Display(Name = "Danh mục")]
    public int DanhMucId { get; set; }

    [ForeignKey(nameof(SachId))]
    public Sach? Sach { get; set; }

    [ForeignKey(nameof(DanhMucId))]
    public DanhMuc? DanhMuc { get; set; }
}