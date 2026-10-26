// Họ và tên: Nguyễn Trung Kiên
// Mã sinh viên: 23274800010
// Nội dung thực hiện: Xây dựng Controller Quản lý Độc giả, Giỏ hàng đa sản phẩm, Form đăng ký mượn/mua sách trực tuyến và hỗ trợ reset mật khẩu.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Data;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Controllers
{
    public class CartItem
    {
        public Sach Sach { get; set; } = null!;
        public int SoLuong { get; set; }
        public string LoaiYeuCau { get; set; } = "Muon"; // "Muon" hoặc "Mua"
    }

    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString("GioHang");
            if (string.IsNullOrEmpty(sessionData)) return new List<CartItem>();

            var itemKeys = sessionData.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var list = new List<CartItem>();
            foreach (var key in itemKeys)
            {
                var parts = key.Split(':');
                if (parts.Length == 3 && int.TryParse(parts[0], out int sachId) && int.TryParse(parts[1], out int qty))
                {
                    var sach = _context.Sachs.Include(s => s.TheLoai).FirstOrDefault(s => s.MaSach == sachId);
                    if (sach != null)
                    {
                        list.Add(new CartItem { Sach = sach, SoLuong = qty, LoaiYeuCau = parts[2] });
                    }
                }
            }
            return list;
        }

        private void SaveCartItems(List<CartItem> list)
        {
            var data = string.Join(";", list.Select(i => $"{i.Sach.MaSach}:{i.SoLuong}:{i.LoaiYeuCau}"));
            HttpContext.Session.SetString("GioHang", data);
        }

        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        public IActionResult ThemVaoGio(int maSach, int soLuong = 1, string loaiYeuCau = "Muon")
        {
            return AddToCart(maSach, loaiYeuCau, soLuong);
        }

        public IActionResult AddToCart(int id, string loai = "Muon", int soLuong = 1)
        {
            var sach = _context.Sachs.Find(id);
            if (sach == null) return NotFound();

            int slActual = soLuong > 0 ? soLuong : 1;
            if (slActual > sach.SoLuongCon)
            {
                slActual = sach.SoLuongCon;
            }

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.Sach.MaSach == id && c.LoaiYeuCau == loai);
            if (item != null)
            {
                item.SoLuong = Math.Min(sach.SoLuongCon, item.SoLuong + slActual);
            }
            else
            {
                cart.Add(new CartItem { Sach = sach, SoLuong = slActual, LoaiYeuCau = loai });
            }

            SaveCartItems(cart);
            TempData["Success"] = $"Đã thêm '{sach.TenSach}' ({slActual} cuốn) vào giỏ {(loai == "Muon" ? "mượn" : "mua")}!";
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id, string loai)
        {
            var cart = GetCartItems();
            cart.RemoveAll(c => c.Sach.MaSach == id && c.LoaiYeuCau == loai);
            SaveCartItems(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (!maTaiKhoan.HasValue)
            {
                TempData["Error"] = "Vui lòng đăng nhập tài khoản Độc giả để gửi yêu cầu!";
                return RedirectToAction("Login", "Account");
            }

            var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
            if (docGia == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin Độc giả tương ứng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            string loaiYeuCau = cart.First().LoaiYeuCau;
            bool isMua = string.Equals(loaiYeuCau, "Mua", StringComparison.OrdinalIgnoreCase);

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = docGia.MaDocGia,
                MaSach = cart.First().Sach.MaSach,
                NgayMuon = DateTime.Now,
                NgayHenTra = isMua ? DateTime.Now : DateTime.Now.AddDays(14),
                TrangThai = "Chờ xác nhận",
                GhiChu = isMua ? "Đăng ký mua sách từ Giỏ hàng" : "Đăng ký mượn sách từ Giỏ hàng"
            };

            _context.PhieuMuons.Add(phieuMuon);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                var chiTiet = new ChiTietPhieuMuon
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaSach = item.Sach.MaSach,
                    SoLuong = item.SoLuong
                };
                _context.ChiTietPhieuMuons.Add(chiTiet);
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("GioHang");

            TempData["Success"] = $"Gửi yêu cầu {(isMua ? "mua" : "mượn")} sách thành công! Vui lòng chờ Quản trị viên duyệt.";
            return RedirectToAction("LichSuMuon", "PhieuMuon");
        }
    }
}