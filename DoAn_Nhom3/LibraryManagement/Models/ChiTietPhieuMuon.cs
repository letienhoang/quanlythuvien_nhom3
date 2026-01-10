using System.ComponentModel.DataAnnotations;
using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class ChiTietPhieuMuon
{
    [Display(Name = "Phiếu mượn")]
    public int PhieuMuonId { get; set; }

    [Display(Name = "Cuốn sách")]
    public int CuonSachId { get; set; }

    [Display(Name = "Ngày trả")]
    public DateTime? NgayTra { get; set; }

    [Display(Name = "Tình trạng trả")]
    public ReturnCondition? TinhTrangTra { get; set; }

    [ForeignKey(nameof(PhieuMuonId))]
    public PhieuMuon? PhieuMuon { get; set; }

    [ForeignKey(nameof(CuonSachId))]
    public CuonSach? CuonSach { get; set; }
}