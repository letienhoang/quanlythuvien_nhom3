using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class HoaDonPhat
{
    [Key] 
    public string MaHoaDon { get; set; }
    public string MaPhat { get; set; }
    public DateTime NgayThanhToan { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; }
}