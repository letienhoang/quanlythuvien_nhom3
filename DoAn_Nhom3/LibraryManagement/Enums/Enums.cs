using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Enums
{
    public enum BookCondition
    {
        [Display(Name = "Mới")]
        Moi, 
        [Display(Name = "Cũ")]
        Cu,     
        [Display(Name = "Hỏng")]
        Hong,   
        [Display(Name = "Mất")]
        Mat     
    }

    public enum CopyStatus
    {
        [Display(Name = "Có sẵn")]
        CoSan,      
        [Display(Name = "Đang mượn")]
        DangMuon,   
        [Display(Name = "Bảo trì")]
        BaoTri      
    }

    public enum ReaderType
    {
        [Display(Name = "Sinh Viên")]
        SinhVien,   
        [Display(Name = "Giảng Viên")]
        GiangVien,
        [Display(Name = "Khách")]
        Khach       
    }

    public enum MemberStatus
    {
        [Display(Name = "Hoạt Động")]
        HoatDong,
        [Display(Name = "Khóa")]
        Khoa
    }

    public enum LoanStatus
    {
        [Display(Name = "Đang Mượn")]
        DangMuon,
        [Display(Name = "Đã Trả Đủ")]
        DaTraDu,
        [Display(Name = "Quá Hạn")]
        QuaHan
    }

    public enum PaymentStatus
    {
        [Display(Name = "Chưa Thanh Toán")]
        ChuaThanhToan,
        [Display(Name = "Đã Thanh Toán")]
        DaThanhToan,
        [Display(Name = "Thanh Toán Một Phần")]
        ThanhToanMotPhan
    }

    public enum ReturnCondition
    {
        [Display(Name = "Nguyên Vẹn")]
        NguyenVen,
        [Display(Name = "Hỏng")]
        Hong,
        [Display(Name = "Mất")]
        Mat
    }

    public enum UserRole
    {
        [Display(Name = "Quản Trị Viên")]
        QuanTriVien,
        [Display(Name = "Thủ Thư")]
        ThuThu
    }

    public enum PaymentMethod
    {
        [Display(Name = "Tiền Mặt")]
        TienMat,
        [Display(Name = "Chuyển Khoản")]
        ChuyenKhoan,
        [Display(Name = "Ví Điện Tử")]
        ViDienTu
    }
}
