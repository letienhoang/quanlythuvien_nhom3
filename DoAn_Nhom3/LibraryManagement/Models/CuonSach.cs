using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class CuonSach
{
    [Key] 
    public string MaCuon { get; set; }
    public string MaSach { get; set; }
    public BookCondition TinhTrang { get; set; }
    public CopyStatus TrangThai { get; set; }
    public string? ViTriKe { get; set; }
    public DateTime? NgayNhap { get; set; }

    [ForeignKey(nameof(MaSach))]
    public Sach? Sach { get; set; }

    public ICollection<ChiTietPhieuMuon>? ChiTietPhieuMuons { get; set; }
}