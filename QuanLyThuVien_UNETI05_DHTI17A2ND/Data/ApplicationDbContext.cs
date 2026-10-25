
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System.Linq;
using System.Reflection.Emit;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<Sach> Sachs { get; set; }
        public DbSet<DocGia> DocGias { get; set; }
        public DbSet<PhieuMuon> PhieuMuons { get; set; }
        public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Khoa chinh ket hop
            modelBuilder.Entity<ChiTietPhieuMuon>()
                .HasKey(c => new
                {
                    c.MaPhieuMuon,
                    c.MaSach
                });

            // Dinh dang gia sach
            modelBuilder.Entity<Sach>()
                .Property(s => s.GiaBia)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sach>()
                .Property(s => s.GiaSach)
                .HasColumnType("decimal(18,2)");

            // Ten dang nhap khong trung
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            // Ten the loai khong trung
            modelBuilder.Entity<TheLoai>()
                .HasIndex(t => t.TenTheLoai)
                .IsUnique();

            // Khong tu dong xoa du lieu lien quan
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior =
                    DeleteBehavior.Restrict;
            }
        }
    }
}
