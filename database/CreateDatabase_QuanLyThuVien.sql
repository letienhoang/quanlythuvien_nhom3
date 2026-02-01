CREATE DATABASE QuanLyThuVien;
GO

USE [QuanLyThuVien];
GO

CREATE TABLE [dbo].[DanhMuc] (                                                                                                                                                                                                           
    [MaDanhMuc] int NOT NULL IDENTITY,                                                                                                                                                                                              
    [TenDanhMuc] nvarchar(250) NULL,                                                                                                                                                                                                
    [MoTa] nvarchar(max) NULL,                                                                                                                                                                                                      
    CONSTRAINT [PK_DanhMuc] PRIMARY KEY ([MaDanhMuc])                                                                                                                                                                              
);
GO                                                                                                                                                                                                                                  
 
CREATE TABLE [dbo].[NguoiMuon] (                                                                                                                                                                                                         
    [MaNguoiMuon] int NOT NULL IDENTITY,                                                                                                                                                                                            
    [HoTen] nvarchar(250) NULL,                                                                                                                                                                                                     
    [NgaySinh] datetime2 NULL,                                                                                                                                                                                                      
    [CCCD] nvarchar(20) NOT NULL,                                                                                                                                                                                                   
    [DiaChi] nvarchar(max) NULL,                                                                                                                                                                                                    
    [SoDienThoai] nvarchar(max) NULL,                                                                                                                                                                                               
    [Email] nvarchar(max) NULL,                                                                                                                                                                                                     
    [LoaiDocGia] nvarchar(20) NOT NULL,                                                                                                                                                                                             
    [NgayDangKy] datetime2 NOT NULL,                                                                                                                                                                                                
    [NgayHetHan] datetime2 NOT NULL,                                                                                                                                                                                                
    [TrangThai] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    CONSTRAINT [PK_NguoiMuon] PRIMARY KEY ([MaNguoiMuon])                                                                                                                                                                          
);
GO
  
CREATE TABLE [dbo].[NhanVien] (                                                                                                                                                                                                          
    [MaNhanVien] int NOT NULL IDENTITY,                                                                                                                                                                                             
    [HoTen] nvarchar(250) NULL,                                                                                                                                                                                                     
    [NgaySinh] datetime2 NULL,                                                                                                                                                                                                      
    [CCCD] nvarchar(20) NULL,                                                                                                                                                                                                       
    [ChucVu] nvarchar(20) NULL,                                                                                                                                                                                                     
    [SoDienThoai] nvarchar(20) NULL,                                                                                                                                                                                                
    [Email] nvarchar(max) NULL,                                                                                                                                                                                                     
    [TaiKhoan] nvarchar(100) NOT NULL,                                                                                                                                                                                              
    [MatKhau] nvarchar(max) NOT NULL,                                                                                                                                                                                               
    CONSTRAINT [PK_NhanVien] PRIMARY KEY ([MaNhanVien])                                                                                                                                                                            
); 
GO                                                                                                                                                                                                                                
  
CREATE TABLE [dbo].[TacGia] (                                                                                                                                                                                                            
    [MaTacGia] int NOT NULL IDENTITY,                                                                                                                                                                                               
    [TenTacGia] nvarchar(250) NOT NULL,                                                                                                                                                                                             
    [NgaySinh] datetime2 NULL,                                                                                                                                                                                                      
    [QuocTich] nvarchar(100) NULL,                                                                                                                                                                                                  
    [MoTa] nvarchar(max) NULL,                                                                                                                                                                                                      
    CONSTRAINT [PK_TacGia] PRIMARY KEY ([MaTacGia])                                                                                                                                                                                
);    
GO                                                                                                                                                                                                                              
 
CREATE TABLE [dbo].[PhieuMuon] (                                                                                                                                                                                                         
    [MaPhieuMuon] int NOT NULL IDENTITY,                                                                                                                                                                                            
    [MaNguoiMuon] int NOT NULL,                                                                                                                                                                                                     
    [MaNhanVien] int NOT NULL,                                                                                                                                                                                                      
    [NgayMuon] datetime2 NOT NULL,                                                                                                                                                                                                  
    [HanTra] datetime2 NOT NULL,                                                                                                                                                                                                    
    [TrangThai] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    CONSTRAINT [PK_PhieuMuon] PRIMARY KEY ([MaPhieuMuon]),                                                                                                                                                                         
    CONSTRAINT [FK_PhieuMuon_NguoiMuon_MaNguoiMuon] FOREIGN KEY ([MaNguoiMuon]) REFERENCES [NguoiMuon] ([MaNguoiMuon]) ON DELETE NO ACTION,                                                                                      
    CONSTRAINT [FK_PhieuMuon_NhanVien_MaNhanVien] FOREIGN KEY ([MaNhanVien]) REFERENCES [NhanVien] ([MaNhanVien]) ON DELETE NO ACTION                                                                                            
);            
GO                                                                                                                                                                                                                      
  
CREATE TABLE [dbo].[Sach] (                                                                                                                                                                                                              
    [MaSach] int NOT NULL IDENTITY,                                                                                                                                                                                                 
    [TenSach] nvarchar(250) NOT NULL,                                                                                                                                                                                               
    [ISBN] nvarchar(50) NOT NULL,                                                                                                                                                                                                   
    [NamXuatBan] int NOT NULL,                                                                                                                                                                                                      
    [NhaXuatBan] nvarchar(200) NULL,                                                                                                                                                                                                
    [NgonNgu] nvarchar(100) NULL,                                                                                                                                                                                                   
    [SoTrang] int NULL,                                                                                                                                                                                                             
    [MoTa] nvarchar(max) NULL,                                                                                                                                                                                                      
    [MaTacGia] int NOT NULL,                                                                                                                                                                                                        
    [SoLuong] int NULL,                                                                                                                                                                                                             
    CONSTRAINT [PK_Sach] PRIMARY KEY ([MaSach]),                                                                                                                                                                                   
    CONSTRAINT [FK_Sach_TacGia_MaTacGia] FOREIGN KEY ([MaTacGia]) REFERENCES [TacGia] ([MaTacGia]) ON DELETE NO ACTION                                                                                                           
);     
GO                                                                                                                                                                                                                             
 
CREATE TABLE [dbo].[PhieuPhat] (                                                                                                                                                                                                         
    [MaPhat] int NOT NULL IDENTITY,                                                                                                                                                                                                 
    [MaPhieuMuon] int NOT NULL,                                                                                                                                                                                                     
    [SoTienPhat] decimal(18,2) NOT NULL,                                                                                                                                                                                            
    [LyDo] nvarchar(max) NULL,                                                                                                                                                                                                      
    [TrangThaiThanhToan] nvarchar(20) NOT NULL,                                                                                                                                                                                     
    CONSTRAINT [PK_PhieuPhat] PRIMARY KEY ([MaPhat]),                                                                                                                                                                              
    CONSTRAINT [FK_PhieuPhat_PhieuMuon_MaPhieuMuon] FOREIGN KEY ([MaPhieuMuon]) REFERENCES [PhieuMuon] ([MaPhieuMuon]) ON DELETE CASCADE                                                                                         
);      
GO                                                                                                                                                                                                                            
  
CREATE TABLE [dbo].[CuonSach] (                                                                                                                                                                                                          
    [MaCuon] int NOT NULL IDENTITY,                                                                                                                                                                                                 
    [MaSach] int NOT NULL,                                                                                                                                                                                                          
    [TinhTrang] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    [TrangThai] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    [ViTriKe] nvarchar(max) NULL,                                                                                                                                                                                                   
    [NgayNhap] datetime2 NULL,                                                                                                                                                                                                      
    CONSTRAINT [PK_CuonSach] PRIMARY KEY ([MaCuon]),                                                                                                                                                                               
    CONSTRAINT [FK_CuonSach_Sach_MaSach] FOREIGN KEY ([MaSach]) REFERENCES [Sach] ([MaSach]) ON DELETE CASCADE                                                                                                                   
);      
GO                                                                                                                                                                                                                            

CREATE TABLE [dbo].[PhanLoai] (                                                                                                                                                                                                          
    [MaSach] int NOT NULL,                                                                                                                                                                                                          
    [MaDanhMuc] int NOT NULL,                                                                                                                                                                                                       
    CONSTRAINT [PK_PhanLoai] PRIMARY KEY ([MaSach], [MaDanhMuc]),                                                                                                                                                                  
    CONSTRAINT [FK_PhanLoai_DanhMuc_MaDanhMuc] FOREIGN KEY ([MaDanhMuc]) REFERENCES [DanhMuc] ([MaDanhMuc]) ON DELETE CASCADE,                                                                                                   
    CONSTRAINT [FK_PhanLoai_Sach_MaSach] FOREIGN KEY ([MaSach]) REFERENCES [Sach] ([MaSach]) ON DELETE CASCADE                                                                                                                   
);        
GO                                                                                                                                                                                                                          
  
CREATE TABLE [dbo].[HoaDonPhat] (                                                                                                                                                                                                        
    [MaHoaDon] int NOT NULL IDENTITY,                                                                                                                                                                                               
    [MaPhat] int NOT NULL,                                                                                                                                                                                                          
    [NgayThanhToan] datetime2 NOT NULL,                                                                                                                                                                                             
    [SoTien] decimal(18,2) NOT NULL,                                                                                                                                                                                                
    [PhuongThuc] nvarchar(20) NOT NULL,                                                                                                                                                                                             
    CONSTRAINT [PK_HoaDonPhat] PRIMARY KEY ([MaHoaDon]),                                                                                                                                                                           
    CONSTRAINT [FK_HoaDonPhat_PhieuPhat_MaPhat] FOREIGN KEY ([MaPhat]) REFERENCES [PhieuPhat] ([MaPhat]) ON DELETE CASCADE                                                                                                       
);       
GO                                                                                                                                                                                                                           

CREATE TABLE [dbo].[ChiTietPhieuMuon] (                                                                                                                                                                                                  
    [MaPhieuMuon] int NOT NULL,                                                                                                                                                                                                     
    [MaCuon] int NOT NULL,                                                                                                                                                                                                          
    [NgayTra] datetime2 NULL,                                                                                                                                                                                                       
    [TinhTrangTra] nvarchar(20) NULL,                                                                                                                                                                                               
    CONSTRAINT [PK_ChiTietPhieuMuon] PRIMARY KEY ([MaPhieuMuon], [MaCuon]),                                                                                                                                                        
    CONSTRAINT [FK_ChiTietPhieuMuon_CuonSach_MaCuon] FOREIGN KEY ([MaCuon]) REFERENCES [CuonSach] ([MaCuon]) ON DELETE NO ACTION,                                                                                                
    CONSTRAINT [FK_ChiTietPhieuMuon_PhieuMuon_MaPhieuMuon] FOREIGN KEY ([MaPhieuMuon]) REFERENCES [PhieuMuon] ([MaPhieuMuon]) ON DELETE CASCADE                                                                                  
);       
GO                                                                                                                                                                                                                           

CREATE INDEX [IX_ChiTietPhieuMuon_MaCuon] ON [ChiTietPhieuMuon] ([MaCuon]);                                                                                                                                                       
  
CREATE INDEX [IX_CuonSach_MaSach] ON [CuonSach] ([MaSach]);                                                                                                                                                                       
 
CREATE INDEX [IX_HoaDonPhat_MaPhat] ON [HoaDonPhat] ([MaPhat]);                                                                                                                                                                   
  
CREATE UNIQUE INDEX [IX_NguoiMuon_CCCD] ON [NguoiMuon] ([CCCD]);                                                                                                                                                                  
 
CREATE UNIQUE INDEX [IX_NhanVien_TaiKhoan] ON [NhanVien] ([TaiKhoan]);                                                                                                                                                            
  
CREATE INDEX [IX_PhanLoai_MaDanhMuc] ON [PhanLoai] ([MaDanhMuc]);                                                                                                                                                                 
  
CREATE INDEX [IX_PhieuMuon_MaNguoiMuon] ON [PhieuMuon] ([MaNguoiMuon]);                                                                                                                                                           
  
CREATE INDEX [IX_PhieuMuon_MaNhanVien] ON [PhieuMuon] ([MaNhanVien]);                                                                                                                                                             
   
CREATE INDEX [IX_PhieuPhat_MaPhieuMuon] ON [PhieuPhat] ([MaPhieuMuon]);                                                                                                                                                           
  
CREATE UNIQUE INDEX [IX_Sach_ISBN] ON [Sach] ([ISBN]);                                                                                                                                                                            
  
CREATE INDEX [IX_Sach_MaTacGia] ON [Sach] ([MaTacGia]);                                                                                                                                                                                                                                                                                                                                                         

GO 