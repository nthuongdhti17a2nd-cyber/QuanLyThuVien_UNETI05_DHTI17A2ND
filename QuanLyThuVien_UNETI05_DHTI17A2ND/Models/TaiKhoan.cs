
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100)]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string VaiTro { get; set; } = "DocGia";

        public bool TrangThai { get; set; } = true;

        public virtual DocGia? DocGia { get; set; }
    }
}

