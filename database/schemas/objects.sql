/* =========================================================
   LIBRARY MANAGEMENT – SQL SERVER
   FUNCTIONS – STORED PROCEDURES – TRIGGERS – CURSORS
   ========================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================================================
   FUNCTIONS
   ========================================================= */

-- 1. Tính số ngày trễ 
-- Trả số ngày trễ lớn hơn 0. Nếu ngày trả là NULL thì trả về ngày hiện tại
CREATE FUNCTION dbo.fn_TinhSoNgayTre
(
    @MaNguoiMuon INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @TotalDaysLate INT;

    SELECT @TotalDaysLate =
           SUM(
                   CASE
                       WHEN ct.NgayTra IS NULL AND p.HanTra < GETDATE()
                           THEN DATEDIFF(day, p.HanTra, GETDATE())
                       WHEN ct.NgayTra IS NOT NULL AND ct.NgayTra > p.HanTra
                           THEN DATEDIFF(day, p.HanTra, ct.NgayTra)
                       ELSE 0
                       END
           )
    FROM ChiTietPhieuMuons ct
             JOIN PhieuMuons p ON ct.MaPhieuMuon = p.Id
    WHERE p.MaNguoiMuon = @MaNguoiMuon;

    RETURN ISNULL(@TotalDaysLate, 0);
END;
GO


-- 2. Đếm số sách đang mượn của độc giả -> (DONE)
-- Đếm số bản ghi cuốn sách mà người mượn hiện đang mượn trong tất cả phiếu mượn
CREATE FUNCTION dbo.fn_SoSachDangMuon
(
    @MaNguoiMuon INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM ChiTietPhieuMuons ct
             JOIN PhieuMuons p ON ct.MaPhieuMuon = p.Id
    WHERE p.MaNguoiMuon = @MaNguoiMuon
      AND ct.NgayTra IS NULL;
    
    RETURN ISNULL(@Count, 0);
END;
GO

-- 3. Đếm số cuốn sách còn sẵn 
-- Trả số bản hiện có sẵn của một sách bằng cách đếm các cuốn sách của nó có trạng thái có sẵn
CREATE FUNCTION dbo.fn_CountAvailableCopies
(
    @MaSach INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM CuonSachs
    WHERE MaSach = @MaSach
      AND TrangThai = 'CoSan';
    
    RETURN ISNULL(@Count, 0);
END;
GO


/* =========================================================
   STORED PROCEDURES
   ========================================================= */

-- 1. Thêm sách và nhập kho -> (DONE)
-- Thêm hoặc cập nhật đầu sách và chèn các bản ghi trong bảng cuốn sách
CREATE PROCEDURE usp_InsertBookAndCopies
(
    @MaSach NVARCHAR(50),
    @TenSach NVARCHAR(250),
    @ISBN NVARCHAR(50),
    @NamXuatBan INT,
    @NhaXuatBan NVARCHAR(200),
    @NgonNgu NVARCHAR(100),
    @SoTrang INT,
    @MoTa NVARCHAR(MAX),
    @MaTacGia INT,
    @SoLuong INT
)
    AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
    BEGIN TRANSACTION;
    
            DECLARE @MaSach INT;
    
    INSERT INTO Sachs
    (
        MaSach, TenSach, ISBN, NamXuatBan,
        NhaXuatBan, NgonNgu, SoTrang, MoTa,
        MaTacGia, SoLuong
    )
    VALUES
        (
            @MaSach, @TenSach, @ISBN, @NamXuatBan,
            @NhaXuatBan, @NgonNgu, @SoTrang, @MoTa,
            @MaTacGia, @SoLuong
        );
    
    SET @MaSach = SCOPE_IDENTITY();
    
            DECLARE @i INT = 1;
            WHILE @i <= @SoLuong
    BEGIN
    INSERT INTO CuonSachs
    (
        MaCuon, MaSach, TinhTrang, TrangThai, ViTriKe, NgayNhap
    )
    VALUES
        (
            CONCAT(@MaSach, '-', @i),
            @MaSach,
            'Moi',
            'CoSan',
            NULL,
            GETDATE()
        );
    
    SET @i += 1;
    END
    
    COMMIT;
    END TRY
    BEGIN CATCH
    ROLLBACK;
            THROW;
    END CATCH
END;
GO



-- 2. Lập phiếu mượn
-- Tạo phiếu mượn và chi tiết mượn, cập nhật trạng thái cuốn sách
CREATE PROCEDURE usp_CreateBorrowRecord
(
    @MaPhieuMuon NVARCHAR(50),
    @MaNguoiMuon INT,
    @MaNhanVien INT,
    @MaCuon INT,
    @HanTra DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fn_SoSachDangMuon(@MaNguoiMuon) >= 3
        BEGIN
            THROW 50001, N'Độc giả đã mượn tối đa 3 cuốn', 1;
        END

    IF NOT EXISTS (
        SELECT 1 FROM CuonSachs
        WHERE Id = @MaCuon AND TrangThai = 'CoSan'
    )
        BEGIN
            THROW 50002, N'Cuốn sách không có sẵn', 1;
        END

    BEGIN TRANSACTION;

    DECLARE @MaPhieuMuon INT;

    INSERT INTO PhieuMuons
    (
        MaPhieuMuon, MaNguoiMuon, MaNhanVien,
        NgayMuon, HanTra, TrangThai
    )
    VALUES
        (
            @MaPhieuMuon, @MaNguoiMuon, @MaNhanVien,
            GETDATE(), @HanTra, 'DangMuon'
        );

    SET @MaPhieuMuon = SCOPE_IDENTITY();

    INSERT INTO ChiTietPhieuMuons
    (
        MaPhieuMuon, MaCuon, NgayTra, TinhTrangTra
    )
    VALUES
        (
            @MaPhieuMuon, @MaCuon, NULL, NULL
        );

    UPDATE CuonSachs
    SET TrangThai = 'DangMuon'
    WHERE Id = @MaCuon;

    COMMIT;
END;
GO

-- 3. Trả sách
-- Cập nhật trạng thái trả cho từng bản ghi chi tiết phiếu mượn, cập nhật cuốn sách, sinh phiếu phạt nếu trễ/hỏng
CREATE PROCEDURE usp_ReturnBooks
(
    @MaPhieuMuon INT,
    @MaCuon INT,
    @TinhTrangTra NVARCHAR(20)
)
    AS
BEGIN
    BEGIN TRANSACTION;
    
    UPDATE ChiTietPhieuMuons
    SET NgayTra = GETDATE(),
        TinhTrangTra = @TinhTrangTra
    WHERE MaPhieuMuon = @MaPhieuMuon
      AND MaCuon = @MaCuon
      AND NgayTra IS NULL;
    
    UPDATE CuonSachs
    SET TrangThai =
            CASE
                WHEN @TinhTrangTra = 'Hong' THEN 'BaoTri'
                WHEN @TinhTrangTra = 'Mat' THEN 'BaoTri'
                ELSE 'CoSan'
                END
    WHERE Id = @MaCuon;
    
    IF NOT EXISTS (
            SELECT 1 FROM ChiTietPhieuMuons
            WHERE MaPhieuMuon = @MaPhieuMuon
              AND NgayTra IS NULL
        )
    BEGIN
    UPDATE PhieuMuons
    SET TrangThai = 'DaTraDu'
    WHERE Id = @MaPhieuMuon;
    END
    
    COMMIT;
END;
GO

-- 4. Gia hạn mượn
-- Gia hạn hạn trả cho một phiếu mượn để tránh bị phạt
CREATE PROCEDURE usp_RenewLoan
(
    @MaPhieuMuon INT,
    @SoNgayGiaHan INT
)
    AS
BEGIN
    UPDATE PhieuMuons
    SET HanTra = DATEADD(DAY, @SoNgayGiaHan, HanTra)
    WHERE Id = @MaPhieuMuon
      AND TrangThai = 'DangMuon';
END;
GO

-- 5. Báo cáo phiếu quá hạn
-- Trả các báo cáo cơ bản dùng các bảng đã liệt kê(danh sách nhiều lượt mượn trong khoảng, danh sách phiếu quá hạn, số sách đang mượn của từng độc giả)
CREATE PROCEDURE usp_GenerateReport
    @FromDate DATETIME = NULL,
    @ToDate   DATETIME = NULL,
    @OnlyOverdue BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.MaPhieuMuon,
        p.MaNguoiMuon,
        n.MaNguoiMuon,
        n.HoTen,
        n.SoDienThoai,
        n.Email,
        p.NgayMuon,
        p.HanTra,
        DATEDIFF(day, p.HanTra, GETUTCDATE()) AS SoNgayTre,
        ISNULL(ct.SoSachDangMuon, 0) AS SoSachDangMuon,
        ISNULL(ph.TongTienPhatChuaTra, 0) AS TongTienPhatChuaTra
    FROM PhieuMuons p
             INNER JOIN NguoiMuons n ON n.Id = p.MaNguoiMuon
             LEFT JOIN (
        SELECT MaPhieuMuon, COUNT(*) AS SoSachDangMuon
        FROM ChiTietPhieuMuons
        WHERE NgayTra IS NULL
        GROUP BY MaPhieuMuon
    ) ct ON ct.MaPhieuMuon = p.Id
             LEFT JOIN (
        SELECT MaPhieuMuon, SUM(SoTienPhat) AS TongTienPhatChuaTra
        FROM PhieuPhats
        WHERE TrangThaiThanhToan <> 'DaThanhToan' 
        GROUP BY MaPhieuMuon
    ) ph ON ph.MaPhieuMuon = p.Id
    WHERE
        p.TrangThai = 'DangMuon'
      AND (@OnlyOverdue = 0 OR p.HanTra < GETUTCDATE())
      AND (@FromDate IS NULL OR p.NgayMuon >= @FromDate)
      AND (@ToDate   IS NULL OR p.NgayMuon <= @ToDate)
    ORDER BY p.HanTra ASC, p.NgayMuon DESC;
END;
GO


/* =========================================================
   TRIGGERS
   ========================================================= */

-- 1. Khi thêm chi tiết mượn
-- Khi thêm chi tiết mượn (tức cuốn được gán cho phiếu mượn), cập nhật trạng thái cuốn sách vừa insert 
CREATE TRIGGER trg_CHITIETPHIEUMUON_AfterInsert
ON ChiTietPhieuMuons
AFTER INSERT
AS
BEGIN
    UPDATE cs
    SET TrangThai = 'DangMuon'
        FROM CuonSachs cs
        JOIN INSERTED i ON cs.Id = i.MaCuon;
END;
GO

-- 2. Khi trả sách
-- Khi một chi tiết mượn được cập nhật ngày trả, cập nhật trạng thái cuốn sách theo tình trạng trả
CREATE TRIGGER trg_CHITIETPHIEUMUON_AfterUpdate
ON ChiTietPhieuMuons
AFTER UPDATE
          AS
BEGIN
    UPDATE cs
    SET TrangThai =
            CASE
                WHEN i.TinhTrangTra IN ('Hong', 'Mat') THEN 'BaoTri'
                ELSE 'CoSan'
                END
        FROM CuonSachs cs
        JOIN INSERTED i ON cs.Id = i.MaCuon
    WHERE i.NgayTra IS NOT NULL;
END;
GO

-- 3. Đồng bộ số lượng sách
-- Khi thêm sửa xóa cuốn sách thì đồng bộ số lượng sách theo số lượng thực tế bản ghi trong bảng cuốn sách
CREATE TRIGGER trg_CUONSACH_AfterInsertUpdateDelete
    ON CuonSachs
    AFTER INSERT, DELETE
AS
BEGIN
    UPDATE s
    SET SoLuong = (
        SELECT COUNT(*) FROM CuonSachs WHERE MaSach = s.Id
    )
        FROM Sachs s
    WHERE s.Id IN (
        SELECT MaSach FROM INSERTED
        UNION
        SELECT MaSach FROM DELETED
        );
END;
GO

-- 4. Mặc định trạng thái phiếu mượn
-- Khi tạo phiếu mượn phải đảm bảo trạng thái phiếu mượn mặc định là đang mượn
CREATE TRIGGER trg_PHIEUMUON_AfterInsert
ON PhieuMuons
AFTER INSERT
AS
BEGIN
    UPDATE PhieuMuons
    SET TrangThai = 'DangMuon'
    WHERE Id IN (SELECT Id FROM INSERTED)
      AND TrangThai IS NULL;
END;
GO

-- 5. Khóa độc giả khi hết hạn
-- Khi sửa người mượn nếu ngày hết hạn nhỏ hơn ngày hiện tại sẽ tự đặt trạng thái không được mượn
CREATE TRIGGER trg_NGUOIMUON_AfterUpdate
ON NguoiMuons
AFTER UPDATE
          AS
BEGIN
    UPDATE NguoiMuons
    SET TrangThai = 'Khoa'
    WHERE Id IN (SELECT Id FROM INSERTED)
      AND NgayHetHan < GETDATE();
END;
GO


/* =========================================================
   CURSOR – STORED PROCEDURES
   ========================================================= */

-- Cursor 1: Cập nhật phiếu mượn quá hạn
-- Duyệt từng phiếu mượn đang mượn có hạn trả bé hơn ngày hiện tại và cập nhật trạng thái của phiếu mượn sang quá hạn (đang thực hiện)
CREATE PROCEDURE sp_ProcessOverdueLoans_Cursor
    AS
BEGIN
    UPDATE PhieuMuons
    SET TrangThai = 'QuaHan'
    WHERE TrangThai = 'DangMuon'
      AND HanTra < GETDATE();
END;
GO

-- Cursor 2: Đồng bộ số lượng sách
-- Duyệt từng sách và cập nhật số lượng sách dựa trên đếm thực tế trong cuốn sách
CREATE PROCEDURE sp_RecalculateBookQuantities_Cursor
    AS
BEGIN
    DECLARE @MaSach INT, @Count INT;

    DECLARE curSach CURSOR FOR
    SELECT Id FROM Sachs;
    
    OPEN curSach;
    FETCH NEXT FROM curSach INTO @MaSach;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
    SELECT @Count = COUNT(*) FROM CuonSachs WHERE MaSach = @MaSach;
    
    UPDATE Sachs
    SET SoLuong = @Count
    WHERE Id = @MaSach;
    
    FETCH NEXT FROM curSach INTO @MaSach;
    END
    
    CLOSE curSach;
    DEALLOCATE curSach;
END;
GO
