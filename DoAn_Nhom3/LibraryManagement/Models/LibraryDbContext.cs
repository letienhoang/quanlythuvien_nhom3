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

        mb.Entity<PhanLoai>()
            .HasKey(x => new { x.SachId, x.DanhMucId });

        mb.Entity<ChiTietPhieuMuon>()
            .HasKey(x => new { x.PhieuMuonId, x.CuonSachId });

        /* =========================
         *  UNIQUE BUSINESS KEYS
         * ========================= */

        mb.Entity<TacGia>()
            .HasIndex(x => x.MaTacGia)
            .IsUnique();

        mb.Entity<Sach>()
            .HasIndex(x => x.MaSach)
            .IsUnique();

        mb.Entity<Sach>()
            .HasIndex(x => x.ISBN)
            .IsUnique();

        mb.Entity<NguoiMuon>()
            .HasIndex(x => x.CCCD)
            .IsUnique();

        mb.Entity<NhanVien>()
            .HasIndex(x => x.TaiKhoan)
            .IsUnique();

        mb.Entity<DanhMuc>()
            .HasIndex(x => x.MaDanhMuc)
            .IsUnique();

        /* =========================
         *  RELATIONSHIPS
         * ========================= */

        // Sach - TacGia (N-1)
        mb.Entity<Sach>()
            .HasOne(s => s.TacGia)
            .WithMany(t => t.Sachs)
            .HasForeignKey(s => s.TacGiaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sach - DanhMuc (N-N)
        mb.Entity<PhanLoai>()
            .HasOne(p => p.Sach)
            .WithMany(s => s.PhanLoais)
            .HasForeignKey(p => p.SachId);

        mb.Entity<PhanLoai>()
            .HasOne(p => p.DanhMuc)
            .WithMany(d => d.PhanLoais)
            .HasForeignKey(p => p.DanhMucId);

        // CuonSach - Sach (N-1)
        mb.Entity<CuonSach>()
            .HasOne(c => c.Sach)
            .WithMany(s => s.CuonSachs)
            .HasForeignKey(c => c.SachId);

        // PhieuMuon - NguoiMuon
        mb.Entity<PhieuMuon>()
            .HasOne(p => p.NguoiMuon)
            .WithMany(n => n.PhieuMuons)
            .HasForeignKey(p => p.NguoiMuonId)
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuMuon - NhanVien
        mb.Entity<PhieuMuon>()
            .HasOne(p => p.NhanVien)
            .WithMany(nv => nv.PhieuMuons)
            .HasForeignKey(p => p.NhanVienId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietPhieuMuon
        mb.Entity<ChiTietPhieuMuon>()
            .HasOne(c => c.PhieuMuon)
            .WithMany(p => p.ChiTietPhieuMuons)
            .HasForeignKey(c => c.PhieuMuonId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ChiTietPhieuMuon>()
            .HasOne(c => c.CuonSach)
            .WithMany(cs => cs.ChiTietPhieuMuons)
            .HasForeignKey(c => c.CuonSachId)
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuPhat - PhieuMuon
        mb.Entity<PhieuPhat>()
            .HasOne(pp => pp.PhieuMuon)
            .WithMany(pm => pm.PhieuPhats)
            .HasForeignKey(pp => pp.PhieuMuonId);

        // HoaDonPhat - PhieuPhat
        mb.Entity<HoaDonPhat>()
            .HasOne(h => h.PhieuPhat)
            .WithMany(p => p.HoaDonPhats)
            .HasForeignKey(h => h.PhieuPhatId);

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
