using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhieuMuon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã phiếu mượn")]
    public int MaPhieuMuon { get; set; }

    // FK -> NguoiMuon
    [Required]
    [Display(Name = "Người mượn")]
    public int MaNguoiMuon { get; set; }

    [ForeignKey(nameof(MaNguoiMuon))]
    public NguoiMuon? NguoiMuon { get; set; }

    // FK -> NhanVien
    [Required]
    [Display(Name = "Nhân viên")]
    public int MaNhanVien { get; set; }

    [ForeignKey(nameof(MaNhanVien))]
    public NhanVien? NhanVien { get; set; }

    [Display(Name = "Ngày mượn")]
    public DateTime NgayMuon { get; set; }

    [Display(Name = "Hạn trả")]
    public DateTime HanTra { get; set; }

    [Display(Name = "Trạng thái")]
    public LoanStatus TrangThai { get; set; }

    public ICollection<ChiTietPhieuMuon>? ChiTietPhieuMuons { get; set; }
    public ICollection<PhieuPhat>? PhieuPhats { get; set; }
}