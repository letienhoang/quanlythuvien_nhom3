namespace LibraryManagement.DtosModels;

public class BorrowReportItem
{
    public int MaPhieuMuon { get; set; }
    public int MaNguoiMuon { get; set; }
    public string? HoTen { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public DateTime NgayMuon { get; set; }
    public DateTime HanTra { get; set; }
    public int SoNgayTre { get; set; }
    public bool IsOverdue { get; set; }
    public int SoSachDangMuon { get; set; }
    public decimal TongTienPhatChuaTra { get; set; }
}