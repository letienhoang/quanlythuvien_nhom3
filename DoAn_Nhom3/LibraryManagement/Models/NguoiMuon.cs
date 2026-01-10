using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class NguoiMuon
{
    [Key]
    public string MaNguoiMuon { get; set; }
    public string HoTen { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string CCCD { get; set; }
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string LoaiDocGia { get; set; }
    public DateTime NgayDangKy { get; set; }
    public DateTime NgayHetHan { get; set; }
    public string TrangThai { get; set; }
}