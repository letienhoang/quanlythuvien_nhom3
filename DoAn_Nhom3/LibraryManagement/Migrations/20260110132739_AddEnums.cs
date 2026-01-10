using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenSach",
                table: "Sach",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NhaXuatBan",
                table: "Sach",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TacGiaMaTacGia",
                table: "Sach",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThaiThanhToan",
                table: "PhieuPhat",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PhieuMuonMaPhieuMuon",
                table: "PhieuPhat",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "PhieuMuon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "NguoiMuonMaNguoiMuon",
                table: "PhieuMuon",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanVienMaNhanVien",
                table: "PhieuMuon",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucMaDanhMuc",
                table: "PhanLoai",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SachMaSach",
                table: "PhanLoai",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChucVu",
                table: "NhanVien",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "NguoiMuon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiDocGia",
                table: "NguoiMuon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhuongThuc",
                table: "HoaDonPhat",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PhieuPhatMaPhat",
                table: "HoaDonPhat",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "CuonSach",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrang",
                table: "CuonSach",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "SachMaSach",
                table: "CuonSach",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrangTra",
                table: "ChiTietPhieuMuon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CuonSachMaCuon",
                table: "ChiTietPhieuMuon",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhieuMuonMaPhieuMuon",
                table: "ChiTietPhieuMuon",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sach_TacGiaMaTacGia",
                table: "Sach",
                column: "TacGiaMaTacGia");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuPhat_PhieuMuonMaPhieuMuon",
                table: "PhieuPhat",
                column: "PhieuMuonMaPhieuMuon");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuMuon_NguoiMuonMaNguoiMuon",
                table: "PhieuMuon",
                column: "NguoiMuonMaNguoiMuon");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuMuon_NhanVienMaNhanVien",
                table: "PhieuMuon",
                column: "NhanVienMaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_PhanLoai_DanhMucMaDanhMuc",
                table: "PhanLoai",
                column: "DanhMucMaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_PhanLoai_SachMaSach",
                table: "PhanLoai",
                column: "SachMaSach");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonPhat_PhieuPhatMaPhat",
                table: "HoaDonPhat",
                column: "PhieuPhatMaPhat");

            migrationBuilder.CreateIndex(
                name: "IX_CuonSach_SachMaSach",
                table: "CuonSach",
                column: "SachMaSach");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuMuon_CuonSachMaCuon",
                table: "ChiTietPhieuMuon",
                column: "CuonSachMaCuon");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuMuon_PhieuMuonMaPhieuMuon",
                table: "ChiTietPhieuMuon",
                column: "PhieuMuonMaPhieuMuon");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuMuon_CuonSach_CuonSachMaCuon",
                table: "ChiTietPhieuMuon",
                column: "CuonSachMaCuon",
                principalTable: "CuonSach",
                principalColumn: "MaCuon");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuMuon_PhieuMuon_PhieuMuonMaPhieuMuon",
                table: "ChiTietPhieuMuon",
                column: "PhieuMuonMaPhieuMuon",
                principalTable: "PhieuMuon",
                principalColumn: "MaPhieuMuon");

            migrationBuilder.AddForeignKey(
                name: "FK_CuonSach_Sach_SachMaSach",
                table: "CuonSach",
                column: "SachMaSach",
                principalTable: "Sach",
                principalColumn: "MaSach");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonPhat_PhieuPhat_PhieuPhatMaPhat",
                table: "HoaDonPhat",
                column: "PhieuPhatMaPhat",
                principalTable: "PhieuPhat",
                principalColumn: "MaPhat");

            migrationBuilder.AddForeignKey(
                name: "FK_PhanLoai_DanhMuc_DanhMucMaDanhMuc",
                table: "PhanLoai",
                column: "DanhMucMaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc");

            migrationBuilder.AddForeignKey(
                name: "FK_PhanLoai_Sach_SachMaSach",
                table: "PhanLoai",
                column: "SachMaSach",
                principalTable: "Sach",
                principalColumn: "MaSach");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuMuon_NguoiMuon_NguoiMuonMaNguoiMuon",
                table: "PhieuMuon",
                column: "NguoiMuonMaNguoiMuon",
                principalTable: "NguoiMuon",
                principalColumn: "MaNguoiMuon");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuMuon_NhanVien_NhanVienMaNhanVien",
                table: "PhieuMuon",
                column: "NhanVienMaNhanVien",
                principalTable: "NhanVien",
                principalColumn: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuPhat_PhieuMuon_PhieuMuonMaPhieuMuon",
                table: "PhieuPhat",
                column: "PhieuMuonMaPhieuMuon",
                principalTable: "PhieuMuon",
                principalColumn: "MaPhieuMuon");

            migrationBuilder.AddForeignKey(
                name: "FK_Sach_TacGia_TacGiaMaTacGia",
                table: "Sach",
                column: "TacGiaMaTacGia",
                principalTable: "TacGia",
                principalColumn: "MaTacGia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuMuon_CuonSach_CuonSachMaCuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuMuon_PhieuMuon_PhieuMuonMaPhieuMuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.DropForeignKey(
                name: "FK_CuonSach_Sach_SachMaSach",
                table: "CuonSach");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonPhat_PhieuPhat_PhieuPhatMaPhat",
                table: "HoaDonPhat");

            migrationBuilder.DropForeignKey(
                name: "FK_PhanLoai_DanhMuc_DanhMucMaDanhMuc",
                table: "PhanLoai");

            migrationBuilder.DropForeignKey(
                name: "FK_PhanLoai_Sach_SachMaSach",
                table: "PhanLoai");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuMuon_NguoiMuon_NguoiMuonMaNguoiMuon",
                table: "PhieuMuon");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuMuon_NhanVien_NhanVienMaNhanVien",
                table: "PhieuMuon");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuPhat_PhieuMuon_PhieuMuonMaPhieuMuon",
                table: "PhieuPhat");

            migrationBuilder.DropForeignKey(
                name: "FK_Sach_TacGia_TacGiaMaTacGia",
                table: "Sach");

            migrationBuilder.DropIndex(
                name: "IX_Sach_TacGiaMaTacGia",
                table: "Sach");

            migrationBuilder.DropIndex(
                name: "IX_PhieuPhat_PhieuMuonMaPhieuMuon",
                table: "PhieuPhat");

            migrationBuilder.DropIndex(
                name: "IX_PhieuMuon_NguoiMuonMaNguoiMuon",
                table: "PhieuMuon");

            migrationBuilder.DropIndex(
                name: "IX_PhieuMuon_NhanVienMaNhanVien",
                table: "PhieuMuon");

            migrationBuilder.DropIndex(
                name: "IX_PhanLoai_DanhMucMaDanhMuc",
                table: "PhanLoai");

            migrationBuilder.DropIndex(
                name: "IX_PhanLoai_SachMaSach",
                table: "PhanLoai");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonPhat_PhieuPhatMaPhat",
                table: "HoaDonPhat");

            migrationBuilder.DropIndex(
                name: "IX_CuonSach_SachMaSach",
                table: "CuonSach");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietPhieuMuon_CuonSachMaCuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietPhieuMuon_PhieuMuonMaPhieuMuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.DropColumn(
                name: "TacGiaMaTacGia",
                table: "Sach");

            migrationBuilder.DropColumn(
                name: "PhieuMuonMaPhieuMuon",
                table: "PhieuPhat");

            migrationBuilder.DropColumn(
                name: "NguoiMuonMaNguoiMuon",
                table: "PhieuMuon");

            migrationBuilder.DropColumn(
                name: "NhanVienMaNhanVien",
                table: "PhieuMuon");

            migrationBuilder.DropColumn(
                name: "DanhMucMaDanhMuc",
                table: "PhanLoai");

            migrationBuilder.DropColumn(
                name: "SachMaSach",
                table: "PhanLoai");

            migrationBuilder.DropColumn(
                name: "PhieuPhatMaPhat",
                table: "HoaDonPhat");

            migrationBuilder.DropColumn(
                name: "SachMaSach",
                table: "CuonSach");

            migrationBuilder.DropColumn(
                name: "CuonSachMaCuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.DropColumn(
                name: "PhieuMuonMaPhieuMuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.AlterColumn<string>(
                name: "TenSach",
                table: "Sach",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "NhaXuatBan",
                table: "Sach",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThaiThanhToan",
                table: "PhieuPhat",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "PhieuMuon",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ChucVu",
                table: "NhanVien",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "NguoiMuon",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "LoaiDocGia",
                table: "NguoiMuon",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PhuongThuc",
                table: "HoaDonPhat",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "CuonSach",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrang",
                table: "CuonSach",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrangTra",
                table: "ChiTietPhieuMuon",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
