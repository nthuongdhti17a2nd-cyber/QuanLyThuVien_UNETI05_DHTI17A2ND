
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class TheLoai
    {
        [Key]
        public int MaTheLoai { get; set; }

        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [StringLength(100)]
        public string TenTheLoai { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<Sach> Sachs { get; set; }
            = new List<Sach>();
    }
}
