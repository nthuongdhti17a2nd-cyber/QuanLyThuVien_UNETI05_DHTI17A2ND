// Họ và tên: Nguyễn Trung Kiên
// Mã sinh viên: 23274800010
// Nội dung thực hiện: Xây dựng Controller Quản lý Độc giả, Giỏ hàng đa sản phẩm, Form đăng ký mượn/mua sách trực tuyến và hỗ trợ reset mật khẩu.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Data;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Controllers
{
    public class DocGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hàm kiểm tra quyền Admin từ Session
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin" || HttpContext.Session.GetString("TenDangNhap") == "admin";
        }

        // Hàm băm mật khẩu SHA256
        private string BamMatKhauSHA256(string matKhau)
        {
            if (string.IsNullOrEmpty(matKhau)) return string.Empty;
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // GET: DocGia (Danh sách & Tìm kiếm)
        public async Task<IActionResult> Index(string searchString)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var docGias = _context.DocGias.Include(d => d.TaiKhoan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string searchTrim = searchString.Trim();
                docGias = docGias.Where(d => d.HoTen.Contains(searchTrim)
                                          || d.MaDocGia.ToString().Contains(searchTrim)
                                          || (d.Email != null && d.Email.Contains(searchTrim))
                                          || (d.SoDienThoai != null && d.SoDienThoai.Contains(searchTrim)));
            }

            ViewBag.SearchString = searchString;
            return View(await docGias.ToListAsync());
        }

        // GET: DocGia/Details/5 (Xem Chi Tiết Độc Giả bao gồm Tên Đăng Nhập)
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaDocGia == id);

            if (docGia == null) return NotFound();

            return View(docGia);
        }

        // POST: DocGia/ResetPassword/5 (Đặt lại mật khẩu = Tên Đăng Nhập của độc giả)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var docGia = await _context.DocGias.Include(d => d.TaiKhoan).FirstOrDefaultAsync(d => d.MaDocGia == id);
            if (docGia == null || docGia.TaiKhoan == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản độc giả!";
                return RedirectToAction(nameof(Index));
            }

            // Lấy chính TenDangNhap làm Mật khẩu mới và băm SHA256
            string tenDangNhap = docGia.TaiKhoan.TenDangNhap;
            docGia.TaiKhoan.MatKhau = BamMatKhauSHA256(tenDangNhap);

            _context.Update(docGia.TaiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã đặt lại mật khẩu cho độc giả '{docGia.HoTen}' trùng với Tên đăng nhập: {tenDangNhap}";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: DocGia/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: DocGia/Create (Tự động tạo TaiKhoan để tránh DbUpdateException do thiếu MaTaiKhoan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocGia docGia, string tenDangNhap, string matKhau)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // 1. Tạo tên đăng nhập tự động nếu bỏ trống
            if (string.IsNullOrWhiteSpace(tenDangNhap))
            {
                if (!string.IsNullOrWhiteSpace(docGia.Email))
                {
                    tenDangNhap = docGia.Email.Split('@')[0];
                }
                else if (!string.IsNullOrWhiteSpace(docGia.SoDienThoai))
                {
                    tenDangNhap = "dg_" + docGia.SoDienThoai;
                }
                else
                {
                    tenDangNhap = "docgia_" + DateTime.Now.Ticks.ToString().Substring(10);
                }
            }

            // Đảm bảo TenDangNhap không bị trùng lặp
            int count = 1;
            string originalTenDangNhap = tenDangNhap;
            while (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
            {
                tenDangNhap = $"{originalTenDangNhap}_{count++}";
            }

            // 2. Mật khẩu mặc định nếu bỏ trống là 123456
            string matKhauThucTe = string.IsNullOrWhiteSpace(matKhau) ? "123456" : matKhau.Trim();
            string matKhauBam = BamMatKhauSHA256(matKhauThucTe);

            // 3. Tạo TaiKhoan mới
            var taiKhoanMoi = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = matKhauBam,
                VaiTro = "DocGia",
                TrangThai = true
            };

            _context.TaiKhoans.Add(taiKhoanMoi);
            await _context.SaveChangesAsync(); // Lưu để phát sinh MaTaiKhoan khóa chính tự tăng

            // 4. Gán MaTaiKhoan vừa tạo cho DocGia
            docGia.MaTaiKhoan = taiKhoanMoi.MaTaiKhoan;

            if (string.IsNullOrWhiteSpace(docGia.DiaChi))
            {
                docGia.DiaChi = "Chưa cập nhật";
            }

            _context.DocGias.Add(docGia);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Thêm độc giả thành công! Tên ĐN: {tenDangNhap} | Mật khẩu: {matKhauThucTe}";
            return RedirectToAction(nameof(Index));
        }

        // GET: DocGia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var docGia = await _context.DocGias.Include(d => d.TaiKhoan).FirstOrDefaultAsync(d => d.MaDocGia == id);
            if (docGia == null) return NotFound();

            return View(docGia);
        }

        // POST: DocGia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocGia docGia)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != docGia.MaDocGia) return NotFound();

            try
            {
                var docGiaDb = await _context.DocGias.FindAsync(id);
                if (docGiaDb == null) return NotFound();

                docGiaDb.HoTen = docGia.HoTen;
                docGiaDb.Email = docGia.Email;
                docGiaDb.SoDienThoai = docGia.SoDienThoai;
                docGiaDb.DiaChi = string.IsNullOrWhiteSpace(docGia.DiaChi) ? "Chưa cập nhật" : docGia.DiaChi;

                _context.Update(docGiaDb);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin độc giả thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
            }

            return View(docGia);
        }

        // GET: DocGia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .Include(d => d.PhieuMuons)
                .FirstOrDefaultAsync(m => m.MaDocGia == id);

            if (docGia == null) return NotFound();

            if (docGia.PhieuMuons != null && docGia.PhieuMuons.Any())
            {
                ViewBag.Warning = "Độc giả này đã từng lập phiếu mượn/mua trong hệ thống. Không thể xóa trực tiếp!";
            }

            return View(docGia);
        }

        // POST: DocGia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Chặn xóa nếu độc giả đã có phiếu mượn trong hệ thống (chống lỗi FK DbUpdateException)
            bool coPhieuMuon = await _context.PhieuMuons.AnyAsync(p => p.MaDocGia == id);
            if (coPhieuMuon)
            {
                TempData["Error"] = "Không thể xóa! Độc giả này đang có dữ liệu phiếu mượn liên kết.";
                return RedirectToAction(nameof(Index));
            }

            var docGia = await _context.DocGias.FindAsync(id);
            if (docGia != null)
            {
                int? maTaiKhoan = docGia.MaTaiKhoan;

                _context.DocGias.Remove(docGia);
                await _context.SaveChangesAsync();

                // Xóa luôn TaiKhoan liên kết nếu có
                if (maTaiKhoan.HasValue)
                {
                    var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
                    if (taiKhoan != null)
                    {
                        _context.TaiKhoans.Remove(taiKhoan);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "Xóa độc giả và tài khoản liên kết thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}