using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class Sach
{
    [Key]
    [Required]
    public string MaSach { get; set; }

    [Required, StringLength(250)]
    public string TenSach { get; set; }

    [Required]
    public string ISBN { get; set; }

    [Range(0, 3000)]
    public int NamXuatBan { get; set; }

    [StringLength(200)]
    public string? NhaXuatBan { get; set; }
    public string? NgonNgu { get; set; }
    public int? SoTrang { get; set; }
    public string? MoTa { get; set; }

    [Required]
    public string MaTacGia { get; set; }

    [ForeignKey(nameof(MaTacGia))]
    public TacGia? TacGia { get; set; }
    public int? SoLuong { get; set; }

    public ICollection<PhanLoai>? PhanLoais { get; set; }
    public ICollection<CuonSach>? CuonSachs { get; set; }
}