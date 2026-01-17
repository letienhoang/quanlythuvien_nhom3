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
    @NguoiMuonId INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM ChiTietPhieuMuons ct
             JOIN PhieuMuons p ON ct.PhieuMuonId = p.Id
    WHERE p.NguoiMuonId = @NguoiMuonId
      AND ct.NgayTra IS NULL;
    
    RETURN ISNULL(@Count, 0);
END;
GO


-- 2. Đếm số sách đang mượn của độc giả
-- Đếm số bản ghi cuốn sách mà người mượn hiện đang mượn trong tất cả phiếu mượn
CREATE FUNCTION dbo.fn_SoSachDangMuon
(
    @NguoiMuonId INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM ChiTietPhieuMuons ct
             JOIN PhieuMuons p ON ct.PhieuMuonId = p.Id
    WHERE p.NguoiMuonId = @NguoiMuonId
      AND ct.NgayTra IS NULL;
    
    RETURN ISNULL(@Count, 0);
END;
GO

-- 3. Đếm số cuốn sách còn sẵn
-- Trả số bản hiện có sẵn của một sách bằng cách đếm các cuốn sách của nó có trạng thái có sẵn
CREATE FUNCTION dbo.fn_CountAvailableCopies
(
    @SachId INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM CuonSachs
    WHERE SachId = @SachId
      AND TrangThai = 'CoSan';
    
    RETURN ISNULL(@Count, 0);
END;
GO


/* =========================================================
   STORED PROCEDURES
   ========================================================= */

-- 1. Thêm sách và nhập kho
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
    @TacGiaId INT,
    @SoLuong INT
)
    AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
    BEGIN TRANSACTION;
    
            DECLARE @SachId INT;
    
    INSERT INTO Sachs
    (
        MaSach, TenSach, ISBN, NamXuatBan,
        NhaXuatBan, NgonNgu, SoTrang, MoTa,
        TacGiaId, SoLuong
    )
    VALUES
        (
            @MaSach, @TenSach, @ISBN, @NamXuatBan,
            @NhaXuatBan, @NgonNgu, @SoTrang, @MoTa,
            @TacGiaId, @SoLuong
        );
    
    SET @SachId = SCOPE_IDENTITY();
    
            DECLARE @i INT = 1;
            WHILE @i <= @SoLuong
    BEGIN
    INSERT INTO CuonSachs
    (
        MaCuon, SachId, TinhTrang, TrangThai, ViTriKe, NgayNhap
    )
    VALUES
        (
            CONCAT(@MaSach, '-', @i),
            @SachId,
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
    @NguoiMuonId INT,
    @NhanVienId INT,
    @CuonSachId INT,
    @HanTra DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fn_SoSachDangMuon(@NguoiMuonId) >= 3
        BEGIN
            THROW 50001, N'Độc giả đã mượn tối đa 3 cuốn', 1;
        END

    IF NOT EXISTS (
        SELECT 1 FROM CuonSachs
        WHERE Id = @CuonSachId AND TrangThai = 'CoSan'
    )
        BEGIN
            THROW 50002, N'Cuốn sách không có sẵn', 1;
        END

    BEGIN TRANSACTION;

    DECLARE @PhieuMuonId INT;

    INSERT INTO PhieuMuons
    (
        MaPhieuMuon, NguoiMuonId, NhanVienId,
        NgayMuon, HanTra, TrangThai
    )
    VALUES
        (
            @MaPhieuMuon, @NguoiMuonId, @NhanVienId,
            GETDATE(), @HanTra, 'DangMuon'
        );

    SET @PhieuMuonId = SCOPE_IDENTITY();

    INSERT INTO ChiTietPhieuMuons
    (
        PhieuMuonId, CuonSachId, NgayTra, TinhTrangTra
    )
    VALUES
        (
            @PhieuMuonId, @CuonSachId, NULL, NULL
        );

    UPDATE CuonSachs
    SET TrangThai = 'DangMuon'
    WHERE Id = @CuonSachId;

    COMMIT;
END;
GO

-- 3. Trả sách
-- Cập nhật trạng thái trả cho từng bản ghi chi tiết phiếu mượn, cập nhật cuốn sách, sinh phiếu phạt nếu trễ/hỏng
CREATE PROCEDURE usp_ReturnBooks
(
    @PhieuMuonId INT,
    @CuonSachId INT,
    @TinhTrangTra NVARCHAR(20)
)
    AS
BEGIN
    BEGIN TRANSACTION;
    
    UPDATE ChiTietPhieuMuons
    SET NgayTra = GETDATE(),
        TinhTrangTra = @TinhTrangTra
    WHERE PhieuMuonId = @PhieuMuonId
      AND CuonSachId = @CuonSachId
      AND NgayTra IS NULL;
    
    UPDATE CuonSachs
    SET TrangThai =
            CASE
                WHEN @TinhTrangTra = 'Hong' THEN 'BaoTri'
                WHEN @TinhTrangTra = 'Mat' THEN 'BaoTri'
                ELSE 'CoSan'
                END
    WHERE Id = @CuonSachId;
    
    IF NOT EXISTS (
            SELECT 1 FROM ChiTietPhieuMuons
            WHERE PhieuMuonId = @PhieuMuonId
              AND NgayTra IS NULL
        )
    BEGIN
    UPDATE PhieuMuons
    SET TrangThai = 'DaTraDu'
    WHERE Id = @PhieuMuonId;
    END
    
    COMMIT;
END;
GO

-- 4. Gia hạn mượn
-- Gia hạn hạn trả cho một phiếu mượn để tránh bị phạt
CREATE PROCEDURE usp_RenewLoan
(
    @PhieuMuonId INT,
    @SoNgayGiaHan INT
)
    AS
BEGIN
    UPDATE PhieuMuons
    SET HanTra = DATEADD(DAY, @SoNgayGiaHan, HanTra)
    WHERE Id = @PhieuMuonId
      AND TrangThai = 'DangMuon';
END;
GO

-- 5. Báo cáo phiếu quá hạn
-- Trả các báo cáo cơ bản dùng các bảng đã liệt kê(danh sách nhiều lượt mượn trong khoảng, danh sách phiếu quá hạn, số sách đang mượn của từng độc giả)
CREATE PROCEDURE usp_GenerateReport
AS
BEGIN
    SELECT
        Id,
        MaPhieuMuon,
        dbo.fn_TinhSoNgayTre(NguoiMuonId) AS SoNgayTre
    FROM PhieuMuons
    WHERE TrangThai = 'DangMuon'
      AND HanTra < GETDATE();
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
        JOIN INSERTED i ON cs.Id = i.CuonSachId;
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
        JOIN INSERTED i ON cs.Id = i.CuonSachId
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
        SELECT COUNT(*) FROM CuonSachs WHERE SachId = s.Id
    )
        FROM Sachs s
    WHERE s.Id IN (
        SELECT SachId FROM INSERTED
        UNION
        SELECT SachId FROM DELETED
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
    DECLARE @PhieuMuonId INT;

    DECLARE cur CURSOR FOR
    SELECT Id
    FROM PhieuMuons
    WHERE TrangThai = 'DangMuon'
      AND HanTra < GETDATE();
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @PhieuMuonId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
    UPDATE PhieuMuons
    SET TrangThai = 'QuaHan'
    WHERE Id = @PhieuMuonId;
    
    FETCH NEXT FROM cur INTO @PhieuMuonId;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END;
GO

-- Cursor 2: Đồng bộ số lượng sách
-- Duyệt từng sách và cập nhật số lượng sách dựa trên đếm thực tế trong cuốn sách
CREATE PROCEDURE sp_RecalculateBookQuantities_Cursor
    AS
BEGIN
    DECLARE @SachId INT, @Count INT;

    DECLARE curSach CURSOR FOR
    SELECT Id FROM Sachs;
    
    OPEN curSach;
    FETCH NEXT FROM curSach INTO @SachId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
    SELECT @Count = COUNT(*) FROM CuonSachs WHERE SachId = @SachId;
    
    UPDATE Sachs
    SET SoLuong = @Count
    WHERE Id = @SachId;
    
    FETCH NEXT FROM curSach INTO @SachId;
    END
    
    CLOSE curSach;
    DEALLOCATE curSach;
END;
GO
