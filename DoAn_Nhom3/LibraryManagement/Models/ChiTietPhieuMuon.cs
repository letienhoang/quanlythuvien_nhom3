using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class ChiTietPhieuMuon
{
    public string MaPhieuMuon { get; set; }
    public string MaCuon { get; set; }
    public DateTime? NgayTra { get; set; }
    public ReturnCondition? TinhTrangTra { get; set; }

    [ForeignKey(nameof(MaPhieuMuon))]
    public PhieuMuon? PhieuMuon { get; set; }

    [ForeignKey(nameof(MaCuon))]
    public CuonSach? CuonSach { get; set; }
}