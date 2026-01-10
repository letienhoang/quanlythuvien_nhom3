using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class NhanVien
{
    [Key] 
    public string MaNhanVien { get; set; }
    public string HoTen { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string? CCCD { get; set; }
    public string? ChucVu { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string TaiKhoan { get; set; }
    public string MatKhau { get; set; }
}