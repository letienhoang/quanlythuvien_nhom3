using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LibraryManagement.DtosModels
{
    public class CreateBorrowRecordDto
    {
        [Required]
        public int MaNguoiMuon { get; set; }

        [Required]
        public int MaNhanVien { get; set; }

        [Required]
        public DateTime HanTra { get; set; }

        // Danh sách mã cuốn sách được chọn để mượn
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một cuốn sách.")]
        public List<int> MaCuons { get; set; } = new();
    }
}
