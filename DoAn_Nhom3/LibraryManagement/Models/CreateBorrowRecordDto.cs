using System.ComponentModel.DataAnnotations;
namespace LibraryManagement.Models
{
    public class CreateBorrowRecordDto
    {
        [Required]
        public string MaPhieuMuon { get; set; } = string.Empty;

        [Required]
        public int NguoiMuonId { get; set; }

        [Required]
        public int NhanVienId { get; set; }

        [Required]
        public int CuonSachId { get; set; }

        [Required]
        public DateTime HanTra { get; set; }
    }
}
