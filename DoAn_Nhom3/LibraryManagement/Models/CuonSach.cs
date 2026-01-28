using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class CuonSach
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã cuốn sách")]
    public int MaCuon { get; set; }

    // FK -> Sach
    [Required]
    [Display(Name = "Sách")]
    public int MaSach { get; set; }

    [ForeignKey(nameof(MaSach))]
    public Sach? Sach { get; set; }

    [Display(Name = "Tình trạng")]
    public BookCondition TinhTrang { get; set; }

    [Display(Name = "Trạng thái")]
    public CopyStatus TrangThai { get; set; }

    [Display(Name = "Vị trí kệ")]
    public string? ViTriKe { get; set; }

    [Display(Name = "Ngày nhập")]
    public DateTime? NgayNhap { get; set; }

    public ICollection<ChiTietPhieuMuon>? ChiTietPhieuMuons { get; set; }
}