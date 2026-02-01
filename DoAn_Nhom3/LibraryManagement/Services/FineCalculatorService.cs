using LibraryManagement.Enums;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public class FineCalculatorService : IFineCalculatorService
    {
        private readonly LibraryDbContext _context;

        private const decimal FINE_PER_DAY = 5000;              // 5,000 VND/ngày
        private const decimal FINE_DAMAGED_BOOK = 200000;       // 200,000 VND (sách hỏng)
        private const decimal FINE_LOST_BOOK = 500000;          // 500,000 VND (sách mất)

        public FineCalculatorService(LibraryDbContext context)
        {
            _context = context;
        }
        public decimal CalculateFine(PhieuMuon phieuMuon)
        {
            if (phieuMuon.TrangThai != LoanStatus.DangMuon && phieuMuon.TrangThai != LoanStatus.QuaHan)
                return 0;

            var today = DateTime.Now.Date;
            if (phieuMuon.HanTra.Date >= today)
                return 0;

            int daysOverdue = (today - phieuMuon.HanTra.Date).Days;
            return daysOverdue * FINE_PER_DAY;
        }

        public decimal CalculateDamageFine(CuonSach cuonSach, ReturnCondition tinhTrang)
        {
            return tinhTrang switch
            {
                ReturnCondition.Hong => FINE_DAMAGED_BOOK,
                ReturnCondition.Mat => FINE_LOST_BOOK,
                _ => 0
            };
        }

        public decimal CalculateTotalFine(PhieuMuon phieuMuon)
        {
            // Tính phạt quá hạn
            decimal overdayFine = CalculateFine(phieuMuon);

            // Tính phạt hỏng/mất sách
            decimal damageFine = 0;
            if (phieuMuon.ChiTietPhieuMuon != null)
            {
                foreach (var chiTiet in phieuMuon.ChiTietPhieuMuon)
                {
                    if (chiTiet.TinhTrangTra.HasValue && chiTiet.TinhTrangTra != ReturnCondition.NguyenVen)
                    {
                        damageFine += CalculateDamageFine(chiTiet.CuonSach, chiTiet.TinhTrangTra.Value);
                    }
                }
            }

            return overdayFine + damageFine;
        }

    }
}
