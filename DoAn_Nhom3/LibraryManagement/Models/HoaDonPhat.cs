using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class HoaDonPhat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã hóa đơn phạt")]
    public int MaHoaDon { get; set; }

    // FK -> PhieuPhat
    [Required]
    [Display(Name = "Phạt")]
    public int MaPhat { get; set; }

    [ForeignKey(nameof(MaPhat))]
    public PhieuPhat? PhieuPhat { get; set; }

    [Display(Name = "Ngày thanh toán")]
    public DateTime NgayThanhToan { get; set; }

    [Display(Name = "Số tiền")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTien { get; set; }

    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PhuongThuc { get; set; }
}