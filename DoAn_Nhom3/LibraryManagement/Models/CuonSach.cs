using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class CuonSach
{
    [Key] public string MaCuon { get; set; }
    public string MaSach { get; set; }
    public string TinhTrang { get; set; }
    public string TrangThai { get; set; }
    public string? ViTriKe { get; set; }
    public DateTime? NgayNhap { get; set; }
}