using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Models;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }
    
    public DbSet<TacGia> TacGia { get; set; }
    public DbSet<DanhMuc> DanhMuc { get; set; }
    public DbSet<Sach> Sach { get; set; }
    public DbSet<PhanLoai> PhanLoai { get; set; }
    public DbSet<CuonSach> CuonSach { get; set; }
    public DbSet<NguoiMuon> NguoiMuon { get; set; }
    public DbSet<NhanVien> NhanVien { get; set; }
    public DbSet<PhieuMuon> PhieuMuon { get; set; }
    public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuon { get; set; }
    public DbSet<PhieuPhat> PhieuPhat { get; set; }
    public DbSet<HoaDonPhat> HoaDonPhat { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // because CuonSach has a trigger in the database.
        mb.Entity<ChiTietPhieuMuon>().ToTable(tb =>
        {
            tb.UseSqlOutputClause(false);
            tb.HasTrigger("trg_CHITIETPHIEUMUON_AfterInsert");
            tb.HasTrigger("trg_CHITIETPHIEUMUON_AfterUpdate");
        });
        
        mb.Entity<CuonSach>().ToTable(tb => {
            tb.UseSqlOutputClause(false);
            tb.HasTrigger("trg_CUONSACH_AfterInsertUpdateDelete");
        });
        
        mb.Entity<PhieuMuon>().ToTable(tb =>
        {
            tb.UseSqlOutputClause(false);
            tb.HasTrigger("trg_PHIEUMUON_AfterInsert");
        });

        mb.Entity<NguoiMuon>().ToTable(tb =>
        {
            tb.UseSqlOutputClause(false);
            tb.HasTrigger("trg_NGUOIMUON_AfterUpdate");
        });
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
            .WithMany(t => t.Sach)
            .HasForeignKey(s => s.MaTacGia)
            .OnDelete(DeleteBehavior.Restrict);

        // Sach - DanhMuc (N-N) qua PhanLoai (PhanLoai.MaSach, PhanLoai.MaDanhMuc)
        mb.Entity<PhanLoai>()
            .HasOne(p => p.Sach)
            .WithMany(s => s.PhanLoai)
            .HasForeignKey(p => p.MaSach);

        mb.Entity<PhanLoai>()
            .HasOne(p => p.DanhMuc)
            .WithMany(d => d.PhanLoai)
            .HasForeignKey(p => p.MaDanhMuc);

        // CuonSach - Sach (N-1) : CuonSach.MaSach -> Sach.MaSach
        mb.Entity<CuonSach>()
            .HasOne(c => c.Sach)
            .WithMany(s => s.CuonSach)
            .HasForeignKey(c => c.MaSach);

        // PhieuMuon - NguoiMuon : PhieuMuon.MaNguoiMuon -> NguoiMuon.MaNguoiMuon
        mb.Entity<PhieuMuon>()
            .HasOne(p => p.NguoiMuon)
            .WithMany(n => n.PhieuMuon)
            .HasForeignKey(p => p.MaNguoiMuon)
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuMuon - NhanVien : PhieuMuon.MaNhanVien -> NhanVien.MaNhanVien
        mb.Entity<PhieuMuon>()
            .HasOne(p => p.NhanVien)
            .WithMany(nv => nv.PhieuMuon)
            .HasForeignKey(p => p.MaNhanVien)
            .OnDelete(DeleteBehavior.Restrict);

        // ChiTietPhieuMuon
        // FK -> PhieuMuon (MaPhieuMuon)
        mb.Entity<ChiTietPhieuMuon>()
            .HasOne(c => c.PhieuMuon)
            .WithMany(p => p.ChiTietPhieuMuon)
            .HasForeignKey(c => c.MaPhieuMuon)
            .OnDelete(DeleteBehavior.Cascade);

        // FK -> CuonSach (MaCuon)
        mb.Entity<ChiTietPhieuMuon>()
            .HasOne(c => c.CuonSach)
            .WithMany(cs => cs.ChiTietPhieuMuon)
            .HasForeignKey(c => c.MaCuon)
            .OnDelete(DeleteBehavior.Restrict);

        // PhieuPhat - PhieuMuon : PhieuPhat.MaPhieuMuon -> PhieuMuon.MaPhieuMuon
        mb.Entity<PhieuPhat>()
            .HasOne(pp => pp.PhieuMuon)
            .WithMany(pm => pm.PhieuPhat)
            .HasForeignKey(pp => pp.MaPhieuMuon);

        // HoaDonPhat - PhieuPhat : HoaDonPhat.MaPhat -> PhieuPhat.MaPhat
        mb.Entity<HoaDonPhat>()
            .HasOne(h => h.PhieuPhat)
            .WithMany(p => p.HoaDonPhat)
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
