namespace LibraryManagement.Models
{
    public class SachThongKeDto
    {
        public int Id { get; set; }
        public string MaSach { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public int TongSoCuon { get; set; }
        public int SoCuonCon { get; set; }
    }
}
