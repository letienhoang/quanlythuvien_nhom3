using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class HoaDonPhat
{
    [Key] 
    public string MaHoaDon { get; set; }
    [Required]
    public string MaPhat { get; set; }
    public DateTime NgayThanhToan { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTien { get; set; }
    public PaymentMethod PhuongThuc { get; set; }

    [ForeignKey(nameof(MaPhat))]
    public PhieuPhat? PhieuPhat { get; set; }
}