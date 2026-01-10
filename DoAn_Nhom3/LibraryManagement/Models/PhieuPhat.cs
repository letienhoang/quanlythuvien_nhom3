using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class PhieuPhat
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Mã phạt")]
    [StringLength(50)]
    public string MaPhat { get; set; }

    // FK -> PhieuMuon
    [Required]
    [Display(Name = "Phiếu mượn")]
    public int PhieuMuonId { get; set; }

    [ForeignKey(nameof(PhieuMuonId))]
    public PhieuMuon? PhieuMuon { get; set; }

    [Display(Name = "Số tiền phạt")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTienPhat { get; set; }

    [Display(Name = "Lý do phạt")]
    public string? LyDo { get; set; }

    [Display(Name = "Trạng thái thanh toán")]
    public PaymentStatus TrangThaiThanhToan { get; set; }

    public ICollection<HoaDonPhat>? HoaDonPhats { get; set; }
}