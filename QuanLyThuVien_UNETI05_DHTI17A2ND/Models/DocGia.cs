
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class DocGia
    {
        [Key]
        public int MaDocGia { get; set; }

        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [StringLength(20)]
        public string? GioiTinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public bool TrangThai { get; set; } = true;

        [ForeignKey(nameof(MaTaiKhoan))]
        public virtual TaiKhoan TaiKhoan { get; set; } = null!;

        public virtual ICollection<PhieuMuon> PhieuMuons
        { get; set; } = new List<PhieuMuon>();
    }
}
