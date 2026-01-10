namespace LibraryManagement.Models;

public class PhanLoai
{
    public string MaSach { get; set; } 
    public string MaDanhMuc { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(MaSach))]
    public Sach? Sach { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(MaDanhMuc))]
    public DanhMuc? DanhMuc { get; set; }
}