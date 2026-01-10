using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class DanhMuc
{ 
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Mã danh mục")]
    [StringLength(50)]
    public string MaDanhMuc { get; set; }

    [Display(Name = "Tên danh mục")]
    [StringLength(250)]
    public string? TenDanhMuc { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    public ICollection<PhanLoai>? PhanLoais { get; set; }
}