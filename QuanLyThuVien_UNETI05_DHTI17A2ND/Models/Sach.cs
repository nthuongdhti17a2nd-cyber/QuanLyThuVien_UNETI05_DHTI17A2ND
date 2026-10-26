// Họ và tên: Nguyễn Trung Kiên
// Mã sinh viên: 23274800010
// Nội dung thực hiện: Model Sách - Validation dữ liệu sách, giá sách, số lượng kho và bổ sung thuộc tính HinhAnh

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class Sach
    {
        [Key]
        [Display(Name = "Mã Sách")]
        public int MaSach { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sách")]
        [StringLength(200, ErrorMessage = "Tên sách không quá 200 ký tự")]
        [Display(Name = "Tên Sách")]
        public string TenSach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thể loại")]
        [Display(Name = "Thể Loại")]
        public int MaTheLoai { get; set; }

        [ForeignKey("MaTheLoai")]
        public virtual TheLoai? TheLoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tác giả")]
        [StringLength(100, ErrorMessage = "Tác giả không quá 100 ký tự")]
        [Display(Name = "Tác Giả")]
        public string TacGia { get; set; } = string.Empty;

        [Display(Name = "Nhà Xuất Bản")]
        public string? NhaXuatBan { get; set; }

        [Range(1000, 2030, ErrorMessage = "Năm xuất bản không hợp lệ")]
        [Display(Name = "Năm Xuất Bản")]
        public int NamXuatBan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổng số lượng")]
        [Range(0, 10000, ErrorMessage = "Số lượng phải >= 0")]
        [Display(Name = "Tổng Số Lượng")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng còn")]
        [Range(0, 10000, ErrorMessage = "Số lượng còn phải >= 0")]
        [Display(Name = "Số Lượng Còn")]
        public int SoLuongCon { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá Bìa")]
        public decimal GiaBia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sách")]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá sách phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá Sách")]
        public decimal GiaSach { get; set; }

        [Display(Name = "Ảnh Bìa")]
        public string? HinhAnh { get; set; }

        [Display(Name = "Mô Tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng Thái")]
        public string? TrangThai { get; set; } = "Đang phục vụ";
    }
}