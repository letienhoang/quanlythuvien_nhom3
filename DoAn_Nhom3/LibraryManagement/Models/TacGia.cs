using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class TacGia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã tác giả")]
    public int MaTacGia { get; set; }

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
