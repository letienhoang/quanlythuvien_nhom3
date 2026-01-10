using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class Sach
{
    [Key] 
    public string MaSach { get; set; }
    public string TenSach { get; set; }
    public string ISBN { get; set; }
    public int NamXuatBan { get; set; }
    public string? NhaXuatBan { get; set; }
    public string? NgonNgu { get; set; }
    public int? SoTrang { get; set; }
    public string? MoTa { get; set; }
    public string MaTacGia { get; set; }
    public int? SoLuong { get; set; }
}