using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class Sach
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã sách")]
    public int MaSach { get; set; }

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
    public int MaTacGia { get; set; }

    [ForeignKey(nameof(MaTacGia))]
    public TacGia? TacGia { get; set; }

    [Display(Name = "Số lượng")]
    public int? SoLuong { get; set; }

    public ICollection<PhanLoai>? PhanLoai { get; set; }
    public ICollection<CuonSach>? CuonSach { get; set; }
}