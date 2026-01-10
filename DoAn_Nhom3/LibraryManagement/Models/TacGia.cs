using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class TacGia
{
    [Key] 
    public string MaTacGia { get; set; } 
    public string TenTacGia { get; set; } 
    public DateTime? NgaySinh { get; set; } 
    public string? QuocTich { get; set; } 
    public string? MoTa { get; set; }
}
