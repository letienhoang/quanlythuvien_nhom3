using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhieuMuon
{
    [Key] 
    public string MaPhieuMuon { get; set; }
    [Required]
    public string MaNguoiMuon { get; set; }
    [Required]
    public string MaNhanVien { get; set; }
    public DateTime NgayMuon { get; set; }
    public DateTime HanTra { get; set; }
    public LoanStatus TrangThai { get; set; }
    public int? SoNgayTre { get; set; }

    [ForeignKey(nameof(MaNguoiMuon))]
    public NguoiMuon? NguoiMuon { get; set; }

    [ForeignKey(nameof(MaNhanVien))]
    public NhanVien? NhanVien { get; set; }

    public ICollection<ChiTietPhieuMuon>? ChiTietPhieuMuons { get; set; }
    public ICollection<PhieuPhat>? PhieuPhats { get; set; }
}