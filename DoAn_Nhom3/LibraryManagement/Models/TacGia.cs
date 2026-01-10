using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class TacGia
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Mã tác giả")]
    [StringLength(50)]
    public string MaTacGia { get; set; }

    [Required]
    [Display(Name = "Tên tác giả")]
    [StringLength(250)]
    public string TenTacGia { get; set; }

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "Quốc tịch")]
    [StringLength(100)]
    public string? QuocTich { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    public ICollection<Sach>? Sachs { get; set; }
}
