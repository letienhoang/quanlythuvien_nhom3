namespace LibraryManagement.Models
{
    public class InsertBookDto
    {
        public string MaSach { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int NamXuatBan { get; set; }
        public string? NhaXuatBan { get; set; }
        public string? NgonNgu { get; set; }
        public int? SoTrang { get; set; }
        public string? MoTa { get; set; }
        public int MaTacGia { get; set; }
        public int SoLuong { get; set; } = 1;
    }
}
