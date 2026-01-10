using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class PhieuMuon
{
    [Key] 
    public string MaPhieuMuon { get; set; }
    public string MaNguoiMuon { get; set; }
    public string MaNhanVien { get; set; }
    public DateTime NgayMuon { get; set; }
    public DateTime HanTra { get; set; }
    public string TrangThai { get; set; }
    public int? SoNgayTre { get; set; }  
}