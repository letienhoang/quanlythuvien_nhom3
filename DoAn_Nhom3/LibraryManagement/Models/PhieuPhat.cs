using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class PhieuPhat
{
    [Key] 
    public string MaPhat { get; set; }
    public string MaPhieuMuon { get; set; }
    public decimal SoTienPhat { get; set; }
    public string? LyDo { get; set; }
    public string TrangThaiThanhToan { get; set; }
}