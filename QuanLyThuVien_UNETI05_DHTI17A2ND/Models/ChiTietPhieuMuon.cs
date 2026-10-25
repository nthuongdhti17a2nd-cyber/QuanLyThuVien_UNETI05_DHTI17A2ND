
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Models
{
    public class ChiTietPhieuMuon
    {
        public int MaPhieuMuon { get; set; }

        public int MaSach { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Số lượng mượn phải > 0")]
        public int SoLuongMuon { get; set; }

        [NotMapped]
        public int SoLuong
        {
            get => SoLuongMuon;
            set => SoLuongMuon = value;
        }

        public DateTime? NgayTra { get; set; }

        [StringLength(100)]
        public string? TinhTrangTra { get; set; }

        [ForeignKey("MaPhieuMuon")]
        public virtual PhieuMuon? PhieuMuon { get; set; }

        [ForeignKey("MaSach")]
        public virtual Sach? Sach { get; set; }
    }
}
