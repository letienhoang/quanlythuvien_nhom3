using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Models;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }
    
    public DbSet<TacGia> TacGias { get; set; }
    public DbSet<DanhMuc> DanhMucs { get; set; }
    public DbSet<Sach> Sachs { get; set; }
    public DbSet<PhanLoai> PhanLoais { get; set; }
    public DbSet<CuonSach> CuonSachs { get; set; }
    public DbSet<NguoiMuon> NguoiMuons { get; set; }
    public DbSet<NhanVien> NhanViens { get; set; }
    public DbSet<PhieuMuon> PhieuMuons { get; set; }
    public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }
    public DbSet<PhieuPhat> PhieuPhats { get; set; }
    public DbSet<HoaDonPhat> HoaDonPhats { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        /* =========================
         *  COMPOSITE KEYS
         * ========================= */

        /* =========================
         *  COMPOSITE KEYS
         * ========================= */

        // PhanLoai: composite key bằng MaSach + MaDanhMuc
        mb.Entity<PhanLoai>()
            .HasKey(x => new { MaSach = x.MaSach, x.MaDanhMuc });

        // ChiTietPhieuMuon: composite key bằng MaPhieuMuon + MaCuon
        mb.Entity<ChiTietPhieuMuon>()
            .HasKey(x => new { x.MaPhieuMuon, x.MaCuon });

        /* =========================
         *  UNIQUE BUSINESS KEYS
         * ========================= */

        // Giữ các unique index hợp lý:
        // ISBN vẫn là unique
        mb.Entity<Sach>()
            .HasIndex(x => x.ISBN)
            .IsUnique();

        // CCCD của NguoiMuon là unique
        mb.Entity<NguoiMuon>()
            .HasIndex(x => x.CCCD)
            .IsUnique();

        // TaiKhoan của NhanVien là unique
        mb.Entity<NhanVien>()
            .HasIndex(x => x.TaiKhoan)
            .IsUnique();

        /* =========================
         *  RELATIONSHIPS (đã chuyển sang dùng các Ma... làm FK)
         * ========================= */

        // Sach - TacGia (N-1) : Sach.MaTacGia -> TacGia.MaTacGia
        mb.Entity<Sach>()
            .HasOne(s => s.TacGia)
            .WithMany(t => t.Sachs)
            .HasForeignKey(s => s.MaTacGia)
            .OnDelete(DeleteBehavior.Restrict);

        // Sach - DanhMuc (N-N) qua PhanLoai (PhanLoai.MaSach, PhanLoai.MaDanhMuc)
        mb.Entity<PhanLoai>()
            .HasOne(p => p.Sach)
            .WithMany(s => s.PhanLoais)
            .HasForeignKey(p => p.MaSach);

        mb.Entity<PhanLoai>()
            .HasOne(p => p.DanhMuc)
            .WithMany(d => d.PhanLoais)
            .HasForeignKey(p => p.MaDanhMuc);

        // CuonSach - Sach (N-1) : CuonSach.MaSach -> Sach.MaSach
        mb.Entity<CuonSach>()
            .HasOne(c => c.Sach)
            .WithMany(s => s.CuonSachs)
            .HasForeignKey(c => c.MaSach);

        // PhieuMuon - NguoiMuon : PhieuMuon.MaNguoiMuon -> NguoiMuon.MaNguoiMuon
        mb.Entity<PhieuMuon>()
            .HasOne(p => p.NguoiMuon)
            .WithMany(n => n.PhieuMuons)
            .HasForeignKey(p => p.MaNguoiMuon)
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuMuon - NhanVien : PhieuMuon.MaNhanVien -> NhanVien.MaNhanVien
        mb.Entity<PhieuMuon>()
            .HasOne(p => p.NhanVien)
            .WithMany(nv => nv.PhieuMuons)
            .HasForeignKey(p => p.MaNhanVien)
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietPhieuMuon
        // FK -> PhieuMuon (MaPhieuMuon)
        mb.Entity<ChiTietPhieuMuon>()
            .HasOne(c => c.PhieuMuon)
            .WithMany(p => p.ChiTietPhieuMuons)
            .HasForeignKey(c => c.MaPhieuMuon)
            .OnDelete(DeleteBehavior.Cascade);

        // FK -> CuonSach (MaCuon)
        mb.Entity<ChiTietPhieuMuon>()
            .HasOne(c => c.CuonSach)
            .WithMany(cs => cs.ChiTietPhieuMuons)
            .HasForeignKey(c => c.MaCuon)
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuPhat - PhieuMuon : PhieuPhat.MaPhieuMuon -> PhieuMuon.MaPhieuMuon
        mb.Entity<PhieuPhat>()
            .HasOne(pp => pp.PhieuMuon)
            .WithMany(pm => pm.PhieuPhats)
            .HasForeignKey(pp => pp.MaPhieuMuon);

        // HoaDonPhat - PhieuPhat : HoaDonPhat.MaPhat -> PhieuPhat.MaPhat
        mb.Entity<HoaDonPhat>()
            .HasOne(h => h.PhieuPhat)
            .WithMany(p => p.HoaDonPhats)
            .HasForeignKey(h => h.MaPhat);

        /* =========================
         *  ENUM → STRING
         * ========================= */

        mb.Entity<CuonSach>().Property(x => x.TinhTrang).HasConversion<string>().HasMaxLength(20);
        mb.Entity<CuonSach>().Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(20);
        mb.Entity<NguoiMuon>().Property(x => x.LoaiDocGia).HasConversion<string>().HasMaxLength(20);
        mb.Entity<NguoiMuon>().Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(20);
        mb.Entity<PhieuMuon>().Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(20);
        mb.Entity<PhieuPhat>().Property(x => x.TrangThaiThanhToan).HasConversion<string>().HasMaxLength(20);
        mb.Entity<ChiTietPhieuMuon>().Property(x => x.TinhTrangTra).HasConversion<string>().HasMaxLength(20);
        mb.Entity<NhanVien>().Property(x => x.ChucVu).HasConversion<string>().HasMaxLength(20);
        mb.Entity<HoaDonPhat>().Property(x => x.PhuongThuc).HasConversion<string>().HasMaxLength(20);

        /* =========================
         *  DECIMAL CONFIG
         * ========================= */

        mb.Entity<HoaDonPhat>()
            .Property(x => x.SoTien)
            .HasColumnType("decimal(18,2)");

        mb.Entity<PhieuPhat>()
            .Property(x => x.SoTienPhat)
            .HasColumnType("decimal(18,2)");
    }
}
