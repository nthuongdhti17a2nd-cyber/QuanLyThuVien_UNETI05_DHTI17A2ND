
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
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

        [Required]
        public int MaDocGia { get; set; }

        public int? MaSach { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayMuon { get; set; }
            = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn hạn trả")]
        [DataType(DataType.Date)]
        public DateTime NgayHenTra { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayTraThucTe { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }
            = "ChoXacNhan";

        [ForeignKey(nameof(MaDocGia))]
        public virtual DocGia DocGia { get; set; } = null!;

        [ForeignKey(nameof(MaSach))]
        public virtual Sach? Sach { get; set; }

        public virtual ICollection<ChiTietPhieuMuon>
            ChiTietPhieuMuons
        { get; set; }
                = new List<ChiTietPhieuMuon>();
    }
}
