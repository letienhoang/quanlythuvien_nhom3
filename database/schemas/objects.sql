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
    @MaNguoiMuon INT,
    @MaCuon INT = NULL
)
    RETURNS INT
AS
BEGIN
    DECLARE @TotalDaysLate INT;

    IF @MaCuon IS NOT NULL
        BEGIN
            SELECT @TotalDaysLate =
                   SUM(
                           CASE
                               WHEN ct.NgayTra IS NULL AND p.HanTra < GETUTCDATE()
                                   THEN DATEDIFF(day, p.HanTra, GETUTCDATE())
                               WHEN ct.NgayTra IS NOT NULL AND ct.NgayTra > p.HanTra
                                   THEN DATEDIFF(day, p.HanTra, ct.NgayTra)
                               ELSE 0
                               END
                   )
            FROM ChiTietPhieuMuons ct
                     JOIN PhieuMuons p ON ct.MaPhieuMuon = p.MaPhieuMuon
            WHERE p.MaNguoiMuon = @MaNguoiMuon
              AND ct.MaCuon = @MaCuon;
        END
    ELSE
        BEGIN
            SELECT @TotalDaysLate = SUM(PerPhieuLate)
            FROM
                (
                    SELECT
                        p.MaPhieuMuon,
                        MAX(
                                CASE
                                    WHEN ct.NgayTra IS NULL AND p.HanTra < GETUTCDATE()
                                        THEN DATEDIFF(day, p.HanTra, GETUTCDATE())
                                    WHEN ct.NgayTra IS NOT NULL AND ct.NgayTra > p.HanTra
                                        THEN DATEDIFF(day, p.HanTra, ct.NgayTra)
                                    ELSE 0
                                    END
                        ) AS PerPhieuLate
                    FROM ChiTietPhieuMuons ct
                             JOIN PhieuMuons p ON ct.MaPhieuMuon = p.MaPhieuMuon
                    WHERE p.MaNguoiMuon = @MaNguoiMuon
                    GROUP BY p.MaPhieuMuon
                ) AS t;
        END

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
             JOIN PhieuMuons p ON ct.MaPhieuMuon = p.MaPhieuMuon
    WHERE p.MaNguoiMuon = @MaNguoiMuon
      AND ct.NgayTra IS NULL AND p.TrangThai = N'DangMuon';

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
      AND TrangThai = N'CoSan';

    RETURN ISNULL(@Count, 0);
END;
GO


/* =========================================================
   STORED PROCEDURES
   ========================================================= */

-- 1. Thêm sách và nhập kho -> (DONE)
-- Thêm hoặc cập nhật đầu sách và chèn các bản ghi trong bảng cuốn sách
-- Thêm phân loại sách trong bảng phân loại

-- Tạo kiểu bảng để truyền danh sách ID danh mục
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

    INSERT INTO Sachs ( TenSach, ISBN, NamXuatBan, NhaXuatBan, NgonNgu, SoTrang, MoTa, MaTacGia, SoLuong )
    VALUES ( @TenSach, @ISBN, @NamXuatBan, @NhaXuatBan, @NgonNgu, @SoTrang, @MoTa, @MaTacGia, @SoLuong );
    
    DECLARE @MaSach INT = CAST(SCOPE_IDENTITY() AS INT);
    
    IF @SoLuong > 0
    BEGIN
        ;WITH Tally AS
        (
            SELECT TOP (@SoLuong) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
            FROM sys.all_objects a CROSS JOIN sys.all_objects b
        )
        INSERT INTO CuonSachs (MaSach, TinhTrang, TrangThai, ViTriKe, NgayNhap)
        SELECT @MaSach, N'Moi', N'CoSan', NULL, GETUTCDATE()
        FROM Tally;
    END
    IF EXISTS (SELECT 1 FROM @MaDanhMucs)
    BEGIN
        INSERT INTO PhanLoais (MaSach, MaDanhMuc)
        SELECT DISTINCT @MaSach, c.Value
        FROM @MaDanhMucs c
        WHERE NOT EXISTS (
            SELECT 1 FROM PhanLoais p
            WHERE p.MaSach = @MaSach AND p.MaDanhMuc = c.Value
        );
    END

    SELECT @MaSach AS MaSach;
END;
GO

-- 2. Lập phiếu mượn
-- Tạo phiếu mượn và chi tiết mượn, cập nhật trạng thái cuốn sách
-- Chỉ tối đa 3 cuốn sách đang mượn, cuốn sách phải có sẵn
CREATE OR ALTER PROCEDURE usp_CreateBorrowRecord (
    @MaNguoiMuon INT,
    @MaNhanVien INT,
    @HanTra DATETIME,
    @MaCuons dbo.IntList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentlyBorrowed INT = dbo.fn_SoSachDangMuon(@MaNguoiMuon);

    DECLARE @RequestedCount INT;
    SELECT @RequestedCount = COUNT(DISTINCT Value) FROM @MaCuons;

    IF @RequestedCount IS NULL OR @RequestedCount = 0
    BEGIN
        RAISERROR(N'Không có cuốn nào được chọn để mượn.', 16, 1);
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
                LEFT JOIN CuonSachs cs
                          ON cs.MaCuon = v.Value AND cs.TrangThai = N'CoSan'
       WHERE cs.MaCuon IS NULL
       FOR XML PATH(''), TYPE
    ).value('.', 'nvarchar(max)'), 1, 1, '');

    IF @MissingIds IS NOT NULL AND LEN(@MissingIds) > 0
    BEGIN
        RAISERROR(N'Những cuốn sau không có sẵn: %s', 16, 1, @MissingIds);
        RETURN;
    END

    INSERT INTO PhieuMuons (MaNguoiMuon, MaNhanVien, NgayMuon, HanTra, TrangThai)
    VALUES (@MaNguoiMuon, @MaNhanVien, GETUTCDATE(), @HanTra, N'DangMuon');

    DECLARE @MaPhieuMuon INT = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO ChiTietPhieuMuons (MaPhieuMuon, MaCuon, NgayTra, TinhTrangTra)
    SELECT @MaPhieuMuon, cs.MaCuon, NULL, NULL
    FROM (
         SELECT DISTINCT Value FROM @MaCuons
    ) v
    INNER JOIN CuonSachs cs
    ON cs.MaCuon = v.Value
       AND cs.TrangThai = N'CoSan';

    UPDATE cs
    SET TrangThai = N'DangMuon'
        FROM CuonSachs cs
            INNER JOIN (
                SELECT DISTINCT Value AS MaCuon FROM @MaCuons
            ) sel ON sel.MaCuon = cs.MaCuon;

    SELECT @MaPhieuMuon AS MaPhieuMuon;
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
    SET NOCOUNT ON;

    UPDATE ChiTietPhieuMuons
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
    
    UPDATE CuonSachs
    SET TrangThai =
    CASE
        WHEN @TinhTrangTra IN (N'Hong', N'Mat') THEN N'BaoTri'
        ELSE N'CoSan'
        END
    WHERE MaCuon = @MaCuon;
    
    DECLARE @HanTra DATETIME = (SELECT p.HanTra FROM PhieuMuons p WHERE p.MaPhieuMuon = @MaPhieuMuon);
    IF @HanTra IS NOT NULL AND GETUTCDATE() > @HanTra
    BEGIN
                DECLARE @DaysLate INT = DATEDIFF(day, @HanTra, GETUTCDATE());
                DECLARE @Amount DECIMAL(18,2) = @DaysLate * 1000;
    INSERT INTO PhieuPhats (MaPhieuMuon, SoTienPhat, LyDo, TrangThaiThanhToan)
    VALUES (@MaPhieuMuon, @Amount, N'Trả muộn ' + CAST(@DaysLate AS NVARCHAR(10)) + N' ngày', N'ChuaThanhToan');
    END
    
    IF NOT EXISTS (
        SELECT 1 FROM ChiTietPhieuMuons
        WHERE MaPhieuMuon = @MaPhieuMuon
          AND NgayTra IS NULL
    )
    BEGIN
        UPDATE PhieuMuons
        SET TrangThai = N'DaTraDu'
        WHERE MaPhieuMuon = @MaPhieuMuon;
    END
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
    
    IF (NOT EXISTS (SELECT 1 FROM PhieuMuons
                    WHERE MaPhieuMuon = @MaPhieuMuon
                      AND TrangThai = N'DangMuon')
    )
    BEGIN
        RAISERROR(N'Phiếu mượn không tồn tại hoặc không ở trạng thái đang mượn.', 16, 1);
        RETURN;
    END

    UPDATE PhieuMuons
    SET HanTra = DATEADD(DAY, @SoNgayGiaHan, HanTra)
    WHERE MaPhieuMuon = @MaPhieuMuon;
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
             INNER JOIN NguoiMuons n ON n.MaNguoiMuon = p.MaNguoiMuon
             LEFT JOIN (
        SELECT MaPhieuMuon, COUNT(*) AS SoSachDangMuon
        FROM ChiTietPhieuMuons
        WHERE NgayTra IS NULL
        GROUP BY MaPhieuMuon
    ) ct ON ct.MaPhieuMuon = p.MaPhieuMuon
             LEFT JOIN (
        SELECT MaPhieuMuon, SUM(SoTienPhat) AS TongTienPhatChuaTra
        FROM PhieuPhats
        WHERE TrangThaiThanhToan <> N'DaThanhToan'
        GROUP BY MaPhieuMuon
    ) ph ON ph.MaPhieuMuon = p.MaPhieuMuon
    WHERE
        p.TrangThai = N'DangMuon'
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
    SET TrangThai = N'DangMuon'
    FROM CuonSachs cs
             JOIN INSERTED i ON cs.MaCuon = i.MaCuon;
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
                WHEN i.TinhTrangTra IN (N'Hong', N'Mat') THEN N'BaoTri'
                ELSE N'CoSan'
                END
    FROM CuonSachs cs
             JOIN INSERTED i ON cs.MaCuon = i.MaCuon
             JOIN DELETED d ON d.MaCuon = i.MaCuon
    WHERE i.NgayTra IS NOT NULL AND d.NgayTra IS NULL;
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
        SELECT COUNT(*) FROM CuonSachs WHERE MaSach = s.MaSach
    )
    FROM Sachs s
    WHERE s.MaSach IN (
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
    SET TrangThai = N'DangMuon'
    WHERE MaPhieuMuon IN (SELECT MaPhieuMuon FROM INSERTED)
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
    SET TrangThai = N'Khoa'
    WHERE MaNguoiMuon IN (SELECT MaNguoiMuon FROM INSERTED)
      AND NgayHetHan < GETUTCDATE();
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
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @MaPhieuMuon INT;

        DECLARE cur_overdue CURSOR LOCAL FAST_FORWARD FOR
        SELECT MaPhieuMuon
        FROM PhieuMuons
        WHERE TrangThai = N'DangMuon'
          AND HanTra < GETUTCDATE();

        OPEN cur_overdue;
            FETCH NEXT FROM cur_overdue INTO @MaPhieuMuon;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                UPDATE PhieuMuons
                SET TrangThai = N'QuaHan'
                WHERE MaPhieuMuon = @MaPhieuMuon
                  AND TrangThai = N'DangMuon'
                  AND HanTra < GETUTCDATE();
                
                FETCH NEXT FROM cur_overdue INTO @MaPhieuMuon;
            END
        CLOSE cur_overdue;
        DEALLOCATE cur_overdue;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('variable', 'cur_overdue') >= -1
        BEGIN
            BEGIN TRY
                CLOSE cur_overdue;
            END TRY
            BEGIN CATCH
            END CATCH
            BEGIN TRY
                DEALLOCATE cur_overdue;
            END TRY
            BEGIN CATCH
            END CATCH
        END
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrNum INT = ERROR_NUMBER();
        RAISERROR(N'Lỗi khi xử lý phiếu quá hạn: %s (Error %d)', 16, 1, @ErrMsg, @ErrNum);
        RETURN;
    END CATCH
END;
GO

-- Cursor 2: Đồng bộ số lượng sách
-- Duyệt từng sách và cập nhật số lượng sách dựa trên đếm thực tế trong cuốn sách
CREATE PROCEDURE sp_RecalculateBookQuantities_Cursor
    AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @MaSach INT;
        DECLARE @Cnt INT;

        DECLARE cur_sach CURSOR LOCAL FAST_FORWARD FOR
        SELECT MaSach FROM Sachs;

        OPEN cur_sach;
            FETCH NEXT FROM cur_sach INTO @MaSach;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                SELECT @Cnt = COUNT(*) FROM CuonSachs WHERE MaSach = @MaSach;
                
                UPDATE Sachs
                SET SoLuong = ISNULL(@Cnt, 0)
                WHERE MaSach = @MaSach;
                
                FETCH NEXT FROM cur_sach INTO @MaSach;
            END

        CLOSE cur_sach;
        DEALLOCATE cur_sach;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('variable', 'cur_sach') >= -1
        BEGIN
            BEGIN TRY
                CLOSE cur_sach;
            END TRY
            BEGIN CATCH
            END CATCH
            BEGIN TRY
                DEALLOCATE cur_sach;
            END TRY
            BEGIN CATCH
            END CATCH
        END

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrNum INT = ERROR_NUMBER();
        RAISERROR(N'Lỗi khi tính lại số lượng sách: %s (Error %d)', 16, 1, @ErrMsg, @ErrNum);
        RETURN;
    END CATCH
END;
GO

