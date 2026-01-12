using LibraryManagement.Enums;
using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    public interface IFineCalculatorService
    {
        /// <summary>
        /// Tính tiền phạt cho phiếu mượn quá hạn
        /// </summary>
        decimal CalculateFine(PhieuMuon phieuMuon);

        /// <summary>
        /// Tính tiền phạt cho sách hỏng/mất
        /// </summary>
        decimal CalculateDamageFine(CuonSach cuonSach, ReturnCondition tinhTrang);

        /// <summary>
        /// Tính tổng tiền phạt (quá hạn + hỏng/mất)
        /// </summary>
        decimal CalculateTotalFine(PhieuMuon phieuMuon);
    }
}
