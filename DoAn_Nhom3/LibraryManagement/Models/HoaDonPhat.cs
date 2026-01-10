using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class HoaDonPhat
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Mã hóa đơn phạt")]
    [StringLength(50)]
    public string MaHoaDon { get; set; }

    // FK -> PhieuPhat
    [Required]
    [Display(Name = "Phạt")]
    public int PhieuPhatId { get; set; }

    [ForeignKey(nameof(PhieuPhatId))]
    public PhieuPhat? PhieuPhat { get; set; }

    [Display(Name = "Ngày thanh toán")]
    public DateTime NgayThanhToan { get; set; }

    [Display(Name = "Số tiền")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTien { get; set; }

    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PhuongThuc { get; set; }
}