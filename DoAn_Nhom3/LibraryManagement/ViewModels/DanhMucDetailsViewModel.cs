namespace LibraryManagement.ViewModels;

public class DanhMucDetailsViewModel
{
    public LibraryManagement.Models.DanhMuc DanhMuc { get; set; } = new();

    // Danh sách sách rút gọn để hiển thị
    public List<BookListItem> Books { get; set; } = new();
    
    public class BookListItem
    {
        public int MaSach { get; set; }
        public string? TenSach { get; set; }
        public string? ISBN { get; set; }
        public string? TacGia { get; set; }
        public int SoLuong { get; set; }        
        public int AvailableCopies { get; set; }
    }
}