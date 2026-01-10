using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhieuPhat
{
    [Key] 
    public string MaPhat { get; set; }
    [Required]
    public string MaPhieuMuon { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTienPhat { get; set; }
    public string? LyDo { get; set; }
    public PaymentStatus TrangThaiThanhToan { get; set; }

    [ForeignKey(nameof(MaPhieuMuon))]
    public PhieuMuon? PhieuMuon { get; set; }

    public ICollection<HoaDonPhat>? HoaDonPhats { get; set; }
}