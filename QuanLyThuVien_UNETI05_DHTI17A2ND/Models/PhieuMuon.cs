// Họ và tên: Nguyễn Trung Kiên
// Mã sinh viên: 23274800010
// Nội dung thực hiện: Model PhieuMuon
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class PhieuMuon
    {
        [Key]
        public int MaPhieuMuon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn độc giả")]
        public int MaDocGia { get; set; }

        [ForeignKey("MaDocGia")]
        public virtual DocGia? DocGia { get; set; }

        public int? MaSach { get; set; }

        [ForeignKey("MaSach")]
        public virtual Sach? Sach { get; set; }

        public DateTime NgayMuon { get; set; } = DateTime.Now;
        public DateTime NgayHenTra { get; set; } = DateTime.Now.AddDays(14);
        public DateTime? NgayTraThucTe { get; set; }

        public string TrangThai { get; set; } = "Chờ xác nhận";
        public string? GhiChu { get; set; }

        // Navigation property liên kết sang ChiTietPhieuMuon
        public virtual ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();
    }
}