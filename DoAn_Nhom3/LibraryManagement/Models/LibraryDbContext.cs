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
        mb.Entity<PhanLoai>().HasKey(x => new { x.MaSach, x.MaDanhMuc });
        mb.Entity<ChiTietPhieuMuon>().HasKey(x => new { x.MaPhieuMuon, x.MaCuon });
        
        mb.Entity<Sach>().HasIndex(x => x.ISBN).IsUnique();
        mb.Entity<NguoiMuon>().HasIndex(x => x.CCCD).IsUnique();
        mb.Entity<NhanVien>().HasIndex(x => x.TaiKhoan).IsUnique();
        
        mb.Entity<PhanLoai>()
          .HasOne<Sach>()
          .WithMany()
          .HasForeignKey(x => x.MaSach)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<PhanLoai>()
          .HasOne<DanhMuc>()
          .WithMany()
          .HasForeignKey(x => x.MaDanhMuc)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Sach>()
          .HasOne<TacGia>()
          .WithMany()
          .HasForeignKey(x => x.MaTacGia)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<CuonSach>()
          .HasOne<Sach>()
          .WithMany()
          .HasForeignKey(x => x.MaSach)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<PhieuMuon>()
          .HasOne<NguoiMuon>()
          .WithMany()
          .HasForeignKey(x => x.MaNguoiMuon)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<PhieuMuon>()
          .HasOne<NhanVien>()
          .WithMany()
          .HasForeignKey(x => x.MaNhanVien)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ChiTietPhieuMuon>()
          .HasOne<PhieuMuon>()
          .WithMany()
          .HasForeignKey(x => x.MaPhieuMuon)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ChiTietPhieuMuon>()
          .HasOne<CuonSach>()
          .WithMany()
          .HasForeignKey(x => x.MaCuon)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<PhieuPhat>()
          .HasOne<PhieuMuon>()
          .WithMany()
          .HasForeignKey(x => x.MaPhieuMuon)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<HoaDonPhat>()
          .HasOne<PhieuPhat>()
          .WithMany()
          .HasForeignKey(x => x.MaPhat)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<HoaDonPhat>()
          .Property(x => x.SoTien)
          .HasColumnType("decimal(18,2)");

        mb.Entity<PhieuPhat>()
          .Property(x => x.SoTienPhat)
          .HasColumnType("decimal(18,2)");
    }
}
