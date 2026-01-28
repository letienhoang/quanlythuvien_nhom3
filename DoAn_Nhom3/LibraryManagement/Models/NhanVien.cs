using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class NhanVien
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã nhân viên")]
    public int MaNhanVien { get; set; }

    [Display(Name = "Họ tên")]
    [StringLength(250)]
    public string? HoTen { get; set; }

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "CCCD")]
    [StringLength(20)]
    public string? CCCD { get; set; }

    [Display(Name = "Chức vụ")]
    public UserRole? ChucVu { get; set; }

    [Display(Name = "Số điện thoại")]
    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Display(Name = "Tài khoản")]
    [StringLength(100)]
    public string TaiKhoan { get; set; }

    [Required]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; }

    public ICollection<PhieuMuon>? PhieuMuons { get; set; }
}