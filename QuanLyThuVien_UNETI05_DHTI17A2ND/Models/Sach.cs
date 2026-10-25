
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class Sach
    {
        [Key]
        public int MaSach { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200)]
        public string TenSach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thể loại")]
        public int MaTheLoai { get; set; }

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(150)]
        public string TacGia { get; set; } = string.Empty;

        [StringLength(150)]
        public string? NhaXuatBan { get; set; }

        [Range(1, 9999, ErrorMessage = "Năm xuất bản không hợp lệ")]
        public int NamXuatBan { get; set; }

        [Range(0, int.MaxValue)]
        public int SoLuong { get; set; }

        [Range(0, int.MaxValue)]
        public int SoLuongCon { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaSach { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaBia { get; set; }

        [StringLength(2000)]
        public string? MoTa { get; set; }

        public string? HinhAnh { get; set; }

        public string TrangThai { get; set; } = "DangPhucVu";

        [ForeignKey(nameof(MaTheLoai))]
        public virtual TheLoai TheLoai { get; set; } = null!;

        public virtual ICollection<ChiTietPhieuMuon>
            ChiTietPhieuMuons
        { get; set; }
                = new List<ChiTietPhieuMuon>();
    }
}
