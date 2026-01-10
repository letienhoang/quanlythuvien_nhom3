using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class Sach
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Mã sách")]
    [StringLength(50)]
    public string MaSach { get; set; }

    [Required, StringLength(250)]
    [Display(Name = "Tên sách")]
    public string TenSach { get; set; }

    [Required]
    [Display(Name = "ISBN")]
    [StringLength(50)]
    public string ISBN { get; set; }

    [Range(0, 3000)]
    [Display(Name = "Năm xuất bản")]
    public int NamXuatBan { get; set; }

    [StringLength(200)]
    [Display(Name = "Nhà xuất bản")]
    public string? NhaXuatBan { get; set; }

    [Display(Name = "Ngôn ngữ")]
    [StringLength(100)]
    public string? NgonNgu { get; set; }

    [Display(Name = "Số trang")]
    public int? SoTrang { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    // FK -> TacGia
    [Required]
    [Display(Name = "Tác giả")]
    public int TacGiaId { get; set; }

    [ForeignKey(nameof(TacGiaId))]
    public TacGia? TacGia { get; set; }

    [Display(Name = "Số lượng")]
    public int? SoLuong { get; set; }

    public ICollection<PhanLoai>? PhanLoais { get; set; }
    public ICollection<CuonSach>? CuonSachs { get; set; }
}