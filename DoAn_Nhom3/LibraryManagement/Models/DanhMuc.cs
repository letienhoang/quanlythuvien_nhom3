using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class DanhMuc
{ 
    [Key] 
    public string MaDanhMuc { get; set; } 
    public string TenDanhMuc { get; set; } 
    public string? MoTa { get; set; } 
}