CREATE DATABASE QuanLyThuVien;
GO

USE [QuanLyThuVien];
GO

CREATE TABLE [dbo].[DanhMucs] (                                                                                                                                                                                                           
    [MaDanhMuc] int NOT NULL IDENTITY,                                                                                                                                                                                              
    [TenDanhMuc] nvarchar(250) NULL,                                                                                                                                                                                                
    [MoTa] nvarchar(max) NULL,                                                                                                                                                                                                      
    CONSTRAINT [PK_DanhMucs] PRIMARY KEY ([MaDanhMuc])                                                                                                                                                                              
);
GO                                                                                                                                                                                                                                  
 
CREATE TABLE [dbo].[NguoiMuons] (                                                                                                                                                                                                         
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
    CONSTRAINT [PK_NguoiMuons] PRIMARY KEY ([MaNguoiMuon])                                                                                                                                                                          
);
GO
  
CREATE TABLE [dbo].[NhanViens] (                                                                                                                                                                                                          
    [MaNhanVien] int NOT NULL IDENTITY,                                                                                                                                                                                             
    [HoTen] nvarchar(250) NULL,                                                                                                                                                                                                     
    [NgaySinh] datetime2 NULL,                                                                                                                                                                                                      
    [CCCD] nvarchar(20) NULL,                                                                                                                                                                                                       
    [ChucVu] nvarchar(20) NULL,                                                                                                                                                                                                     
    [SoDienThoai] nvarchar(20) NULL,                                                                                                                                                                                                
    [Email] nvarchar(max) NULL,                                                                                                                                                                                                     
    [TaiKhoan] nvarchar(100) NOT NULL,                                                                                                                                                                                              
    [MatKhau] nvarchar(max) NOT NULL,                                                                                                                                                                                               
    CONSTRAINT [PK_NhanViens] PRIMARY KEY ([MaNhanVien])                                                                                                                                                                            
); 
GO                                                                                                                                                                                                                                
  
CREATE TABLE [dbo].[TacGias] (                                                                                                                                                                                                            
    [MaTacGia] int NOT NULL IDENTITY,                                                                                                                                                                                               
    [TenTacGia] nvarchar(250) NOT NULL,                                                                                                                                                                                             
    [NgaySinh] datetime2 NULL,                                                                                                                                                                                                      
    [QuocTich] nvarchar(100) NULL,                                                                                                                                                                                                  
    [MoTa] nvarchar(max) NULL,                                                                                                                                                                                                      
    CONSTRAINT [PK_TacGias] PRIMARY KEY ([MaTacGia])                                                                                                                                                                                
);    
GO                                                                                                                                                                                                                              
 
CREATE TABLE [dbo].[PhieuMuons] (                                                                                                                                                                                                         
    [MaPhieuMuon] int NOT NULL IDENTITY,                                                                                                                                                                                            
    [MaNguoiMuon] int NOT NULL,                                                                                                                                                                                                     
    [MaNhanVien] int NOT NULL,                                                                                                                                                                                                      
    [NgayMuon] datetime2 NOT NULL,                                                                                                                                                                                                  
    [HanTra] datetime2 NOT NULL,                                                                                                                                                                                                    
    [TrangThai] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    CONSTRAINT [PK_PhieuMuons] PRIMARY KEY ([MaPhieuMuon]),                                                                                                                                                                         
    CONSTRAINT [FK_PhieuMuons_NguoiMuons_MaNguoiMuon] FOREIGN KEY ([MaNguoiMuon]) REFERENCES [NguoiMuons] ([MaNguoiMuon]) ON DELETE NO ACTION,                                                                                      
    CONSTRAINT [FK_PhieuMuons_NhanViens_MaNhanVien] FOREIGN KEY ([MaNhanVien]) REFERENCES [NhanViens] ([MaNhanVien]) ON DELETE NO ACTION                                                                                            
);            
GO                                                                                                                                                                                                                      
  
CREATE TABLE [dbo].[Sachs] (                                                                                                                                                                                                              
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
    CONSTRAINT [PK_Sachs] PRIMARY KEY ([MaSach]),                                                                                                                                                                                   
    CONSTRAINT [FK_Sachs_TacGias_MaTacGia] FOREIGN KEY ([MaTacGia]) REFERENCES [TacGias] ([MaTacGia]) ON DELETE NO ACTION                                                                                                           
);     
GO                                                                                                                                                                                                                             
 
CREATE TABLE [dbo].[PhieuPhats] (                                                                                                                                                                                                         
    [MaPhat] int NOT NULL IDENTITY,                                                                                                                                                                                                 
    [MaPhieuMuon] int NOT NULL,                                                                                                                                                                                                     
    [SoTienPhat] decimal(18,2) NOT NULL,                                                                                                                                                                                            
    [LyDo] nvarchar(max) NULL,                                                                                                                                                                                                      
    [TrangThaiThanhToan] nvarchar(20) NOT NULL,                                                                                                                                                                                     
    CONSTRAINT [PK_PhieuPhats] PRIMARY KEY ([MaPhat]),                                                                                                                                                                              
    CONSTRAINT [FK_PhieuPhats_PhieuMuons_MaPhieuMuon] FOREIGN KEY ([MaPhieuMuon]) REFERENCES [PhieuMuons] ([MaPhieuMuon]) ON DELETE CASCADE                                                                                         
);      
GO                                                                                                                                                                                                                            
  
CREATE TABLE [dbo].[CuonSachs] (                                                                                                                                                                                                          
    [MaCuon] int NOT NULL IDENTITY,                                                                                                                                                                                                 
    [MaSach] int NOT NULL,                                                                                                                                                                                                          
    [TinhTrang] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    [TrangThai] nvarchar(20) NOT NULL,                                                                                                                                                                                              
    [ViTriKe] nvarchar(max) NULL,                                                                                                                                                                                                   
    [NgayNhap] datetime2 NULL,                                                                                                                                                                                                      
    CONSTRAINT [PK_CuonSachs] PRIMARY KEY ([MaCuon]),                                                                                                                                                                               
    CONSTRAINT [FK_CuonSachs_Sachs_MaSach] FOREIGN KEY ([MaSach]) REFERENCES [Sachs] ([MaSach]) ON DELETE CASCADE                                                                                                                   
);      
GO                                                                                                                                                                                                                            

CREATE TABLE [dbo].[PhanLoais] (                                                                                                                                                                                                          
    [MaSach] int NOT NULL,                                                                                                                                                                                                          
    [MaDanhMuc] int NOT NULL,                                                                                                                                                                                                       
    CONSTRAINT [PK_PhanLoais] PRIMARY KEY ([MaSach], [MaDanhMuc]),                                                                                                                                                                  
    CONSTRAINT [FK_PhanLoais_DanhMucs_MaDanhMuc] FOREIGN KEY ([MaDanhMuc]) REFERENCES [DanhMucs] ([MaDanhMuc]) ON DELETE CASCADE,                                                                                                   
    CONSTRAINT [FK_PhanLoais_Sachs_MaSach] FOREIGN KEY ([MaSach]) REFERENCES [Sachs] ([MaSach]) ON DELETE CASCADE                                                                                                                   
);        
GO                                                                                                                                                                                                                          
  
CREATE TABLE [dbo].[HoaDonPhats] (                                                                                                                                                                                                        
    [MaHoaDon] int NOT NULL IDENTITY,                                                                                                                                                                                               
    [MaPhat] int NOT NULL,                                                                                                                                                                                                          
    [NgayThanhToan] datetime2 NOT NULL,                                                                                                                                                                                             
    [SoTien] decimal(18,2) NOT NULL,                                                                                                                                                                                                
    [PhuongThuc] nvarchar(20) NOT NULL,                                                                                                                                                                                             
    CONSTRAINT [PK_HoaDonPhats] PRIMARY KEY ([MaHoaDon]),                                                                                                                                                                           
    CONSTRAINT [FK_HoaDonPhats_PhieuPhats_MaPhat] FOREIGN KEY ([MaPhat]) REFERENCES [PhieuPhats] ([MaPhat]) ON DELETE CASCADE                                                                                                       
);       
GO                                                                                                                                                                                                                           

CREATE TABLE [dbo].[ChiTietPhieuMuons] (                                                                                                                                                                                                  
    [MaPhieuMuon] int NOT NULL,                                                                                                                                                                                                     
    [MaCuon] int NOT NULL,                                                                                                                                                                                                          
    [NgayTra] datetime2 NULL,                                                                                                                                                                                                       
    [TinhTrangTra] nvarchar(20) NULL,                                                                                                                                                                                               
    CONSTRAINT [PK_ChiTietPhieuMuons] PRIMARY KEY ([MaPhieuMuon], [MaCuon]),                                                                                                                                                        
    CONSTRAINT [FK_ChiTietPhieuMuons_CuonSachs_MaCuon] FOREIGN KEY ([MaCuon]) REFERENCES [CuonSachs] ([MaCuon]) ON DELETE NO ACTION,                                                                                                
    CONSTRAINT [FK_ChiTietPhieuMuons_PhieuMuons_MaPhieuMuon] FOREIGN KEY ([MaPhieuMuon]) REFERENCES [PhieuMuons] ([MaPhieuMuon]) ON DELETE CASCADE                                                                                  
);       
GO                                                                                                                                                                                                                           

CREATE INDEX [IX_ChiTietPhieuMuons_MaCuon] ON [ChiTietPhieuMuons] ([MaCuon]);                                                                                                                                                       
  
CREATE INDEX [IX_CuonSachs_MaSach] ON [CuonSachs] ([MaSach]);                                                                                                                                                                       
 
CREATE INDEX [IX_HoaDonPhats_MaPhat] ON [HoaDonPhats] ([MaPhat]);                                                                                                                                                                   
  
CREATE UNIQUE INDEX [IX_NguoiMuons_CCCD] ON [NguoiMuons] ([CCCD]);                                                                                                                                                                  
 
CREATE UNIQUE INDEX [IX_NhanViens_TaiKhoan] ON [NhanViens] ([TaiKhoan]);                                                                                                                                                            
  
CREATE INDEX [IX_PhanLoais_MaDanhMuc] ON [PhanLoais] ([MaDanhMuc]);                                                                                                                                                                 
  
CREATE INDEX [IX_PhieuMuons_MaNguoiMuon] ON [PhieuMuons] ([MaNguoiMuon]);                                                                                                                                                           
  
CREATE INDEX [IX_PhieuMuons_MaNhanVien] ON [PhieuMuons] ([MaNhanVien]);                                                                                                                                                             
   
CREATE INDEX [IX_PhieuPhats_MaPhieuMuon] ON [PhieuPhats] ([MaPhieuMuon]);                                                                                                                                                           
  
CREATE UNIQUE INDEX [IX_Sachs_ISBN] ON [Sachs] ([ISBN]);                                                                                                                                                                            
  
CREATE INDEX [IX_Sachs_MaTacGia] ON [Sachs] ([MaTacGia]);                                                                                                                                                                                                                                                                                                                                                         

GO 