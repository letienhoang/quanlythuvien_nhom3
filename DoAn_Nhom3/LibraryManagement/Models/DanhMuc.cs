using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class DanhMuc
{ 
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã danh mục")]
    public int MaDanhMuc { get; set; }

    [Display(Name = "Tên danh mục")]
    [StringLength(250)]
    public string? TenDanhMuc { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    public ICollection<PhanLoai>? PhanLoai { get; set; }
}