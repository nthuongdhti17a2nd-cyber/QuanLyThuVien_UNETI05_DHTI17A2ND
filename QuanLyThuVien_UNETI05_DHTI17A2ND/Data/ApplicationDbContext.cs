// Họ và tên: Nguyễn Trung Kiên
// Mã sinh viên: 23274800010
// Nội dung thực hiện: Khai báo DbContext đầy đủ 6 bảng & Cấu hình khóa ngoại không xung đột
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System.Linq;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Khai báo đầy đủ 6 bảng chính
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<Sach> Sachs { get; set; }
        public DbSet<DocGia> DocGias { get; set; }
        public DbSet<PhieuMuon> PhieuMuons { get; set; }
        public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình khóa chính kết hợp cho ChiTietPhieuMuon (MaPhieuMuon, MaSach)
            modelBuilder.Entity<ChiTietPhieuMuon>()
                .HasKey(c => new { c.MaPhieuMuon, c.MaSach });

            // 2. Định dạng kiểu decimal cho giá sách
            modelBuilder.Entity<Sach>()
                .Property(s => s.GiaBia)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sach>()
                .Property(s => s.GiaSach)
                .HasColumnType("decimal(18,2)");

            // 3. TẮT TỰ ĐỘNG XÓA DÂY CHUYỀN (CASCADE DELETE)
            // Giúp tránh hoàn toàn lỗi FOREIGN KEY constraint (Introducing FOREIGN KEY constraint may cause cycles)
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}