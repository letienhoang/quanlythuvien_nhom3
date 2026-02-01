using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhieuPhat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã phạt")]
    public int MaPhat { get; set; }

    // FK -> PhieuMuon
    [Required]
    [Display(Name = "Phiếu mượn")]
    public int MaPhieuMuon { get; set; }

    [ForeignKey(nameof(MaPhieuMuon))]
    public PhieuMuon? PhieuMuon { get; set; }

    [Display(Name = "Số tiền phạt")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTienPhat { get; set; }

    [Display(Name = "Lý do phạt")]
    public string? LyDo { get; set; }

    [Display(Name = "Trạng thái thanh toán")]
    public PaymentStatus TrangThaiThanhToan { get; set; }

    public ICollection<HoaDonPhat>? HoaDonPhat { get; set; }
}