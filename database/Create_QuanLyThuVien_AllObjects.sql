USE [QuanLyThuVien];
GO

/* =========================================================
   FUNCTIONS
   ========================================================= */

-- Function 1. Tính số ngày trễ 
CREATE FUNCTION dbo.fn_CountDaysLate
(
    @MaPhieuMuon INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @DaysLate INT;
    SELECT @DaysLate = DATEDIFF(day, p.HanTra, GETUTCDATE())
    FROM PhieuMuon p
    WHERE p.MaPhieuMuon = @MaPhieuMuon
      AND p.HanTra < GETUTCDATE();

    IF @DaysLate IS NULL OR @DaysLate <= 0
        RETURN 0;

    RETURN ISNULL(@DaysLate, 0);
END;
GO


-- Function 2. Đếm số sách đang mượn của độc giả
CREATE FUNCTION dbo.fn_CountBooksCurrentlyBorrowed
(
    @MaNguoiMuon INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM ChiTietPhieuMuon ct
             JOIN PhieuMuon p ON ct.MaPhieuMuon = p.MaPhieuMuon
    WHERE p.MaNguoiMuon = @MaNguoiMuon
      AND ct.NgayTra IS NULL AND p.TrangThai = N'DangMuon';

    RETURN ISNULL(@Count, 0);
END;
GO

-- Function 3. Đếm số cuốn sách còn sẵn 
CREATE FUNCTION dbo.fn_CountAvailableCopies
(
    @MaSach INT
)
    RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM CuonSach
    WHERE MaSach = @MaSach
      AND TrangThai = N'CoSan';

    RETURN ISNULL(@Count, 0);
END;
GO

-- Function 4. Tính tiền phạt chưa thanh toán
CREATE FUNCTION dbo.fn_CalculateUnpaidFines
(
    @MaPhieuMuon INT
)
    RETURNS DECIMAL(18, 2)
AS
BEGIN
    DECLARE @DaysLate INT;
    SELECT @DaysLate = dbo.fn_CountDaysLate(@MaPhieuMuon);
    
    IF @DaysLate IS NULL OR @DaysLate <= 0
        RETURN 0;
        
    DECLARE @TotalFine DECIMAL(18, 2);
    Set @TotalFine = @DaysLate * 10000;
    RETURN ISNULL(@TotalFine, 0);
END;
GO

/* =========================================================
   STORED PROCEDURES
   ========================================================= */

-- Stored Procedure 1. Thêm sách và nhập kho
    
-- Tạo kiểu bảng để truyền danh sách mã danh mục
IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'IntList')
BEGIN
    CREATE TYPE dbo.IntList AS TABLE
        (
        Value INT NOT NULL
        );
END
GO

CREATE PROCEDURE usp_InsertBookAndCopies
(
    @TenSach NVARCHAR(250),
    @ISBN NVARCHAR(50),
    @NamXuatBan INT,
    @NhaXuatBan NVARCHAR(200),
    @NgonNgu NVARCHAR(100),
    @SoTrang INT,
    @MoTa NVARCHAR(MAX),
    @MaTacGia INT,
    @SoLuong INT,
    @MaDanhMucs dbo.IntList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @SoLuong IS NULL OR @SoLuong < 0
        SET @SoLuong = 0;

    INSERT INTO Sach ( TenSach, ISBN, NamXuatBan, NhaXuatBan, NgonNgu, SoTrang, MoTa, MaTacGia, SoLuong )
    VALUES ( @TenSach, @ISBN, @NamXuatBan, @NhaXuatBan, @NgonNgu, @SoTrang, @MoTa, @MaTacGia, @SoLuong );
    
    DECLARE @MaSach INT = CAST(SCOPE_IDENTITY() AS INT);
    
    IF @SoLuong > 0
    BEGIN
        ;WITH Tally AS
        (
            SELECT TOP (@SoLuong) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
            FROM sys.all_objects a CROSS JOIN sys.all_objects b
        )
        INSERT INTO CuonSach (MaSach, TinhTrang, TrangThai, ViTriKe, NgayNhap)
        SELECT @MaSach, N'Moi', N'CoSan', NULL, GETUTCDATE()
        FROM Tally;
    END
    IF EXISTS (SELECT 1 FROM @MaDanhMucs)
    BEGIN
        INSERT INTO PhanLoai (MaSach, MaDanhMuc)
        SELECT DISTINCT @MaSach, c.Value
        FROM @MaDanhMucs c
        WHERE NOT EXISTS (
            SELECT 1 FROM PhanLoai p
            WHERE p.MaSach = @MaSach AND p.MaDanhMuc = c.Value
        );
    END

    SELECT @MaSach AS MaSach;
END;
GO

-- Stored Procedure 2. Lập phiếu mượn
CREATE PROCEDURE usp_CreateBorrowRecord (
    @MaNguoiMuon INT,
    @MaNhanVien INT,
    @HanTra DATETIME,
    @MaCuons dbo.IntList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentlyBorrowed INT = dbo.fn_CountBooksCurrentlyBorrowed(@MaNguoiMuon);

    DECLARE @RequestedCount INT;
    SELECT @RequestedCount = COUNT(DISTINCT Value) FROM @MaCuons;

    IF @RequestedCount IS NULL OR @RequestedCount = 0
    BEGIN
        RAISERROR(N'Không có cuốn nào được chọn để mượn.', 16, 1);
        RETURN;
    END
    
    IF EXISTS(SELECT 1 FROM NguoiMuon WHERE MaNguoiMuon = @MaNguoiMuon AND TrangThai = N'Khoa')
    BEGIN
        RAISERROR(N'Độc giả hiện đang bị khóa, không thể mượn sách.', 16, 1);
        RETURN;
    END

    IF (@CurrentlyBorrowed + @RequestedCount) > 3
    BEGIN
        RAISERROR(N'Độc giả đã vượt giới hạn mượn (tối đa 3 cuốn).', 16, 1);
        RETURN;
    END

    DECLARE @MissingIds NVARCHAR(MAX);

    SELECT @MissingIds = STUFF((
       SELECT ',' + CAST(v.Value AS NVARCHAR(20))
       FROM (
                SELECT DISTINCT Value FROM @MaCuons
            ) v
                LEFT JOIN CuonSach cs
                          ON cs.MaCuon = v.Value AND cs.TrangThai = N'CoSan'
       WHERE cs.MaCuon IS NULL
       FOR XML PATH(''), TYPE
    ).value('.', 'nvarchar(max)'), 1, 1, '');

    IF @MissingIds IS NOT NULL AND LEN(@MissingIds) > 0
    BEGIN
        RAISERROR(N'Những cuốn sau không có sẵn: %s', 16, 1, @MissingIds);
        RETURN;
    END

    INSERT INTO PhieuMuon (MaNguoiMuon, MaNhanVien, NgayMuon, HanTra, TrangThai)
    VALUES (@MaNguoiMuon, @MaNhanVien, GETUTCDATE(), @HanTra, N'DangMuon');

    DECLARE @MaPhieuMuon INT = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO ChiTietPhieuMuon (MaPhieuMuon, MaCuon, NgayTra, TinhTrangTra)
    SELECT @MaPhieuMuon, cs.MaCuon, NULL, NULL
    FROM (
         SELECT DISTINCT Value FROM @MaCuons
    ) v
    INNER JOIN CuonSach cs
    ON cs.MaCuon = v.Value
       AND cs.TrangThai = N'CoSan';

    UPDATE cs
    SET TrangThai = N'DangMuon'
        FROM CuonSach cs
            INNER JOIN (
                SELECT DISTINCT Value AS MaCuon FROM @MaCuons
            ) sel ON sel.MaCuon = cs.MaCuon;

    SELECT @MaPhieuMuon AS MaPhieuMuon;
END;
GO

-- Stored Procedure 3. Trả sách
CREATE PROCEDURE usp_ReturnBooks
(
    @MaPhieuMuon INT,
    @MaCuon INT,
    @TinhTrangTra NVARCHAR(20)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ChiTietPhieuMuon
    SET NgayTra = GETUTCDATE(),
        TinhTrangTra = @TinhTrangTra
    WHERE MaPhieuMuon = @MaPhieuMuon
      AND MaCuon = @MaCuon
      AND NgayTra IS NULL;
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR(N'Không tìm thấy bản ghi mượn hợp lệ để trả',16,1);
        RETURN;
    END
    
    UPDATE CuonSach
    SET TrangThai =
    CASE
        WHEN @TinhTrangTra IN (N'Hong', N'Mat') THEN N'BaoTri'
        ELSE N'CoSan'
        END
    WHERE MaCuon = @MaCuon;
    
    DECLARE @HanTra DATETIME = (SELECT p.HanTra FROM PhieuMuon p WHERE p.MaPhieuMuon = @MaPhieuMuon);
    IF @HanTra IS NOT NULL AND GETUTCDATE() > @HanTra
    BEGIN
		DECLARE @DaysLate INT = DATEDIFF(day, @HanTra, GETUTCDATE());
		DECLARE @Amount DECIMAL(18,2) = @DaysLate * 1000;
    INSERT INTO PhieuPhat (MaPhieuMuon, SoTienPhat, LyDo, TrangThaiThanhToan)
    VALUES (@MaPhieuMuon, @Amount, N'Trả muộn ' + CAST(@DaysLate AS NVARCHAR(10)) + N' ngày', N'ChuaThanhToan');
    END
    
    IF NOT EXISTS (
        SELECT 1 FROM ChiTietPhieuMuon
        WHERE MaPhieuMuon = @MaPhieuMuon
          AND NgayTra IS NULL
    )
    BEGIN
        UPDATE PhieuMuon
        SET TrangThai = N'DaTraDu'
        WHERE MaPhieuMuon = @MaPhieuMuon;
    END
END;
GO

-- Stored Procedure 4. Gia hạn mượnStored Procedure 
CREATE PROCEDURE usp_RenewLoan
(
    @MaPhieuMuon INT,
    @SoNgayGiaHan INT
)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF (@SoNgayGiaHan IS NULL OR @SoNgayGiaHan <= 0)
    BEGIN
        RAISERROR(N'Số ngày gia hạn phải lớn hơn 0.', 16, 1);
        RETURN;
    END
    
    IF (@SoNgayGiaHan > 365)
    BEGIN
        RAISERROR(N'Số ngày gia hạn không được vượt quá 365 ngày.', 16, 1);
        RETURN;
    END 
    
    IF (NOT EXISTS (SELECT 1 FROM PhieuMuon
                    WHERE MaPhieuMuon = @MaPhieuMuon
                      AND TrangThai = N'DangMuon')
    )
    BEGIN
        RAISERROR(N'Phiếu mượn không tồn tại hoặc không ở trạng thái đang mượn.', 16, 1);
        RETURN;
    END

    UPDATE PhieuMuon
    SET HanTra = DATEADD(DAY, @SoNgayGiaHan, HanTra)
    WHERE MaPhieuMuon = @MaPhieuMuon;
END;
GO

-- Stored Procedure 5. Báo cáo phiếu quá hạn
CREATE PROCEDURE usp_GenerateReport
    @FromDate DATETIME = NULL,
    @ToDate   DATETIME = NULL,
    @OnlyOverdue BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.MaPhieuMuon,
        p.MaNguoiMuon,
        n.HoTen,
        n.SoDienThoai,
        n.Email,
        p.NgayMuon,
        p.HanTra,
        dbo.fn_CountDaysLate(p.MaPhieuMuon) as SoNgayTre,
        CAST(CASE WHEN p.HanTra < GETUTCDATE() THEN 1 ELSE 0 END AS bit) AS IsOverdue,
        ISNULL(ct.SoSachDangMuon, 0) AS SoSachDangMuon,
        ISNULL(ph.TongTienPhatChuaTra, 0) AS TongTienPhatChuaTra
    FROM PhieuMuon p
	INNER JOIN NguoiMuon n ON n.MaNguoiMuon = p.MaNguoiMuon
	LEFT JOIN (
        SELECT MaPhieuMuon, COUNT(*) AS SoSachDangMuon
        FROM ChiTietPhieuMuon
        WHERE NgayTra IS NULL
        GROUP BY MaPhieuMuon
    ) ct ON ct.MaPhieuMuon = p.MaPhieuMuon
             LEFT JOIN (
        SELECT MaPhieuMuon, SUM(SoTienPhat) AS TongTienPhatChuaTra
        FROM PhieuPhat
        WHERE TrangThaiThanhToan <> N'DaThanhToan'
        GROUP BY MaPhieuMuon
    ) ph ON ph.MaPhieuMuon = p.MaPhieuMuon
    WHERE (@OnlyOverdue = 0 OR p.HanTra < GETUTCDATE())
      AND (@FromDate IS NULL OR p.NgayMuon >= @FromDate)
      AND (@ToDate   IS NULL OR p.NgayMuon <= @ToDate)
    ORDER BY p.HanTra ASC, p.NgayMuon DESC;
END;
GO

-- Stored Procedure 6. Cập nhật tiền phạt tăng theo ngày
CREATE PROCEDURE usp_UpdateUnpaidFines
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE pp
    SET SoTienPhat = dbo.fn_CalculateUnpaidFines(pp.MaPhieuMuon)
    FROM PhieuPhat pp
    JOIN PhieuMuon pm ON pp.MaPhieuMuon = pm.MaPhieuMuon
    WHERE pp.TrangThaiThanhToan != N'DaThanhToan';
END;
GO


/* =========================================================
   TRIGGERS
   ========================================================= */

-- Trigger 1. Khi mượn sách
CREATE TRIGGER trg_CHITIETPHIEUMUON_AfterInsert
    ON ChiTietPhieuMuon
    AFTER INSERT
    AS
BEGIN
    UPDATE cs
    SET TrangThai = N'DangMuon'
    FROM CuonSach cs
             JOIN INSERTED i ON cs.MaCuon = i.MaCuon;
END;
GO

-- Trigger 2. Khi trả sách
CREATE TRIGGER trg_CHITIETPHIEUMUON_AfterUpdate
    ON ChiTietPhieuMuon
    AFTER UPDATE
    AS
BEGIN
    UPDATE cs
    SET TrangThai =
            CASE
                WHEN i.TinhTrangTra IN (N'Hong', N'Mat') THEN N'BaoTri'
                ELSE N'CoSan'
                END
    FROM CuonSach cs
	JOIN INSERTED i ON cs.MaCuon = i.MaCuon
	JOIN DELETED d ON d.MaCuon = i.MaCuon
    WHERE i.NgayTra IS NOT NULL AND d.NgayTra IS NULL;
END;
GO

-- Trigger 3. Cập nhật số lượng sách
CREATE TRIGGER trg_CUONSACH_AfterInsertUpdateDelete
    ON CuonSach
    AFTER INSERT, DELETE
    AS
BEGIN
    UPDATE s
    SET SoLuong = (
        SELECT COUNT(*) FROM CuonSach WHERE MaSach = s.MaSach
    )
    FROM Sach s
    WHERE s.MaSach IN (
        SELECT MaSach FROM INSERTED
        UNION
        SELECT MaSach FROM DELETED
    );
END;
GO

-- Trigger 4. Đặt trạng thái phiếu mượn khi tạo mới
CREATE TRIGGER trg_PHIEUMUON_AfterInsert
    ON PhieuMuon
    AFTER INSERT
    AS
BEGIN
    UPDATE PhieuMuon
    SET TrangThai = N'DangMuon'
    WHERE MaPhieuMuon IN (SELECT MaPhieuMuon FROM INSERTED)
      AND TrangThai IS NULL;
END;
GO

-- Trigger 5. Khóa độc giả khi hết hạn
CREATE TRIGGER trg_NGUOIMUON_AfterUpdate
    ON NguoiMuon
    AFTER UPDATE
    AS
BEGIN
    UPDATE NguoiMuon
    SET TrangThai = N'Khoa'
    WHERE MaNguoiMuon IN (SELECT MaNguoiMuon FROM INSERTED)
      AND NgayHetHan < GETUTCDATE();
END;
GO


/* =========================================================
   CURSOR – STORED PROCEDURES
   ========================================================= */

-- Cursor 1: Xử lý phiếu mượn quá hạn
CREATE PROCEDURE sp_ProcessOverdueLoans_Cursor
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaPhieuMuon INT;

    DECLARE cur_overdue CURSOR LOCAL FAST_FORWARD FOR
        SELECT PM.MaPhieuMuon
        FROM PhieuMuon PM
                 LEFT JOIN PhieuPhat PP ON PM.MaPhieuMuon = PP.MaPhieuMuon
        WHERE PM.TrangThai = N'DangMuon'
          AND PM.HanTra < GETUTCDATE()
          AND PP.MaPhieuMuon IS NULL;

    OPEN cur_overdue;
    FETCH NEXT FROM cur_overdue INTO @MaPhieuMuon;

    WHILE @@FETCH_STATUS = 0
        BEGIN
            UPDATE PhieuMuon
            SET TrangThai = N'QuaHan'
            WHERE MaPhieuMuon = @MaPhieuMuon
              AND TrangThai = N'DangMuon'
              AND HanTra < GETUTCDATE();
            
            INSERT INTO PhieuPhat (
                MaPhieuMuon,
                SoTienPhat,
                LyDo,
                TrangThaiThanhToan
            ) VALUES (
                         @MaPhieuMuon,
                         dbo.fn_CalculateUnpaidFines(@MaPhieuMuon),
                         N'Phiếu mượn quá hạn',
                         N'ChuaThanhToan'
                     );

            FETCH NEXT FROM cur_overdue INTO @MaPhieuMuon;
        END

    CLOSE cur_overdue;
    DEALLOCATE cur_overdue;
END;
GO

-- Cursor 2: Tính lại số lượng sách
CREATE PROCEDURE sp_RecalculateBookQuantities_Cursor
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaSach INT;
    DECLARE @Cnt INT;

    DECLARE cur_sach CURSOR LOCAL FAST_FORWARD FOR
        SELECT MaSach FROM Sach;

    OPEN cur_sach;
    FETCH NEXT FROM cur_sach INTO @MaSach;

    WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @Cnt = COUNT(*)
            FROM CuonSach
            WHERE MaSach = @MaSach;

            UPDATE Sach
            SET SoLuong = ISNULL(@Cnt, 0)
            WHERE MaSach = @MaSach;

            FETCH NEXT FROM cur_sach INTO @MaSach;
        END

    CLOSE cur_sach;
    DEALLOCATE cur_sach;
END;
GO

