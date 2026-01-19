namespace LibraryManagement.Models
{
    public class SachQuaHanReportDto
    {
        public int Id { get; set; }
        public string MaPhieuMuon { get; set; }

        public int NguoiMuonId { get; set; }
        public string MaNguoiMuon { get; set; }
        public string HoTen { get; set; }

        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; }
        public int TongSoNgayTre { get; set; }

        public int SoNgayTre { get; set; }
        public int SoSachDangMuon { get; set; }
        public decimal TongTienPhatChuaTra { get; set; }

    }
}
