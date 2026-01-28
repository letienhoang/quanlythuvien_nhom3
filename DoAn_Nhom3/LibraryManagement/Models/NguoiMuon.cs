using LibraryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

public class NguoiMuon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã người mượn")]
    public int MaNguoiMuon { get; set; }

    [Display(Name = "Họ tên")]
    [StringLength(250)]
    public string? HoTen { get; set; }

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }

    [Required]
    [Display(Name = "CCCD")]
    [StringLength(20)]
    public string CCCD { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Loại độc giả")]
    public ReaderType LoaiDocGia { get; set; }

    [Display(Name = "Ngày đăng ký")]
    public DateTime NgayDangKy { get; set; }

    [Display(Name = "Ngày hết hạn")]
    public DateTime NgayHetHan { get; set; }

    [Display(Name = "Trạng thái")]
    public MemberStatus TrangThai { get; set; }

    public ICollection<PhieuMuon>? PhieuMuons { get; set; }
}