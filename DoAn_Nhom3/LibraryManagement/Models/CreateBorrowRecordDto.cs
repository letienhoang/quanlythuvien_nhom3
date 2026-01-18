using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LibraryManagement.Models
{
    public class CreateBorrowRecordDto
    {
        [Required]
        public string MaPhieuMuon { get; set; }

        [Required]
        public int NguoiMuonId { get; set; }

        [Required]
        public int NhanVienId { get; set; }

        [Required]
        public DateTime HanTra { get; set; }

        // Danh sách mã cuốn sách được chọn để mượn
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một cuốn sách.")]
        public List<int> CuonSachIds { get; set; } = new();
    }
}
