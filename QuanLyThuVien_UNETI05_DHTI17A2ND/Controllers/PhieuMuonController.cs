// Họ và tên: Nguyễn Thị Hương
// Mã sinh viên: 23103100067
// Nội dung thực hiện: Xây dựng Controller Dashboard Quản trị, Phân hệ Quản lý mượn trả sách (Duyệt đơn, Trả sách, Hủy phiếu) và Thống kê báo cáo dữ liệu.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Services;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Data;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Controllers
{
    public class PhieuMuonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhieuMuonController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin" || HttpContext.Session.GetString("TenDangNhap") == "admin";
        }

        public async Task<IActionResult> Index(string searchString, string trangThai, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var query = _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string searchTrim = searchString.Trim();
                query = query.Where(p => p.DocGia != null && p.DocGia.HoTen.Contains(searchTrim));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var list = await query
                .OrderByDescending(p => p.NgayMuon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.MaDocGia = new SelectList(await _context.DocGias.ToListAsync(), "MaDocGia", "HoTen");
            ViewBag.MaSach = new SelectList(await _context.Sachs.Where(s => s.SoLuongCon > 0).ToListAsync(), "MaSach", "TenSach");

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateTime.Now,
                NgayHenTra = DateTime.Now.AddDays(14),
                TrangThai = "Đang mượn"
            };

            return View(phieuMuon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuMuon phieuMuon, List<int> danhSachSachId, List<int> danhSachSoLuong, List<string> danhSachNgayHenTra)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (danhSachSachId == null || !danhSachSachId.Any() || danhSachSachId.All(id => id == 0))
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một cuốn sách để lập phiếu.");
                ViewBag.MaDocGia = new SelectList(await _context.DocGias.ToListAsync(), "MaDocGia", "HoTen", phieuMuon.MaDocGia);
                ViewBag.MaSach = new SelectList(await _context.Sachs.Where(s => s.SoLuongCon > 0).ToListAsync(), "MaSach", "TenSach");
                return View(phieuMuon);
            }

            try
            {
                phieuMuon.NgayMuon = DateTime.Now;

                if (danhSachNgayHenTra != null && danhSachNgayHenTra.Any() && DateTime.TryParse(danhSachNgayHenTra[0], out DateTime parsedDate))
                {
                    phieuMuon.NgayHenTra = parsedDate;
                }
                else
                {
                    phieuMuon.NgayHenTra = DateTime.Now.AddDays(14);
                }

                phieuMuon.TrangThai = "Đang mượn";
                phieuMuon.GhiChu = string.IsNullOrWhiteSpace(phieuMuon.GhiChu) ? "Admin lập phiếu mượn tại quầy" : phieuMuon.GhiChu.Trim();

                // Gán mã sách đầu tiên vào bảng chính để thỏa mãn ràng buộc Not-Null của CSDL
                if (danhSachSachId != null && danhSachSachId.Any(id => id > 0))
                {
                    phieuMuon.MaSach = danhSachSachId.First(id => id > 0);
                }

                // 1. Lưu phiếu mượn chính
                _context.PhieuMuons.Add(phieuMuon);
                await _context.SaveChangesAsync();

                // 2. Lưu chi tiết các sách mượn và trừ tồn kho
                for (int i = 0; i < danhSachSachId.Count; i++)
                {
                    int sachId = danhSachSachId[i];
                    if (sachId > 0)
                    {
                        int soLuong = (danhSachSoLuong != null && danhSachSoLuong.Count > i) ? danhSachSoLuong[i] : 1;

                        var chiTiet = new ChiTietPhieuMuon
                        {
                            MaPhieuMuon = phieuMuon.MaPhieuMuon,
                            MaSach = sachId,
                            SoLuong = soLuong
                        };
                        _context.ChiTietPhieuMuons.Add(chiTiet);

                        var sach = await _context.Sachs.FindAsync(sachId);
                        if (sach != null)
                        {
                            sach.SoLuongCon = Math.Max(0, sach.SoLuongCon - soLuong);
                            _context.Sachs.Update(sach);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Lập phiếu mượn sách thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Lỗi CSDL: " + innerError);

                ViewBag.MaDocGia = new SelectList(await _context.DocGias.ToListAsync(), "MaDocGia", "HoTen", phieuMuon.MaDocGia);
                ViewBag.MaSach = new SelectList(await _context.Sachs.Where(s => s.SoLuongCon > 0).ToListAsync(), "MaSach", "TenSach");
                return View(phieuMuon);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .FirstOrDefaultAsync(m => m.MaPhieuMuon == id);

            if (phieuMuon == null) return NotFound();

            if (!IsAdmin())
            {
                var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
                if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

                var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
                if (docGia == null || phieuMuon.MaDocGia != docGia.MaDocGia)
                {
                    TempData["Error"] = "Bạn không có quyền xem phiếu này!";
                    return RedirectToAction(nameof(LichSuMuon));
                }
            }

            return View(phieuMuon);
        }

        public async Task<IActionResult> LichSuMuon()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

            var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
            if (docGia == null) return RedirectToAction("Login", "Account");

            var list = await _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .Where(p => p.MaDocGia == docGia.MaDocGia)
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> TaoPhieuMuon(int id)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null) return NotFound();

            var phieuMuon = new PhieuMuon
            {
                MaSach = id,
                Sach = sach,
                NgayMuon = DateTime.Now,
                NgayHenTra = DateTime.Now.AddDays(14),
                TrangThai = "Chờ xác nhận"
            };

            return View(phieuMuon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieuMuon(int maSach, string ghiChu, int soLuong = 1)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

            var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
            if (docGia == null) return RedirectToAction("Login", "Account");

            var sach = await _context.Sachs.FindAsync(maSach);
            if (sach == null || sach.SoLuongCon <= 0)
            {
                TempData["Error"] = "Sách này hiện đã hết trong kho!";
                return RedirectToAction("Index", "Sach");
            }

            if (soLuong > sach.SoLuongCon)
            {
                soLuong = sach.SoLuongCon;
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = docGia.MaDocGia,
                MaSach = maSach,
                NgayMuon = DateTime.Now,
                NgayHenTra = DateTime.Now.AddDays(14),
                TrangThai = "Chờ xác nhận",
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? "Đăng ký mượn sách qua Website" : ghiChu.Trim()
            };

            _context.PhieuMuons.Add(phieuMuon);
            await _context.SaveChangesAsync();

            var chiTiet = new ChiTietPhieuMuon
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                MaSach = maSach,
                SoLuong = soLuong
            };
            _context.ChiTietPhieuMuons.Add(chiTiet);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(docGia.Email))
            {
                string bodyHtml = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #0d6efd; border-radius: 12px; max-width: 600px; margin: auto;'>
                        <h2 style='color: #0d6efd; text-align: center; border-bottom: 2px solid #0d6efd; padding-bottom: 10px;'>ĐÃ TIẾP NHẬN YÊU CẦU MƯỢN SÁCH</h2>
                        <p>Xin chào <b>{docGia.HoTen}</b>,</p>
                        <p>Hệ thống Trung Tâm Thư Viện đã tiếp nhận yêu cầu đăng ký mượn sách của bạn. Đang <b><span style='color: #fd7e14;'>Chờ Quản trị viên duyệt</span></b>.</p>
                        <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Mã phiếu:</td><td style='padding: 8px;'>PM-{phieuMuon.MaPhieuMuon}</td></tr>
                            <tr style='background-color: #f8f9fa;'><td style='padding: 8px; font-weight: bold;'>Tên sách:</td><td style='padding: 8px;'>{sach.TenSach}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Số lượng:</td><td style='padding: 8px;'>{soLuong} cuốn</td></tr>
                        </table>
                    </div>";

                _ = EmailService.SendEmailAsync(docGia.Email, "[Trung Tâm Thư Viện] ĐÃ TIẾP NHẬN YÊU CẦU MƯỢN SÁCH", bodyHtml);
            }

            TempData["Success"] = "Đã gửi yêu cầu mượn sách thành công!";
            return RedirectToAction(nameof(LichSuMuon));
        }

        [HttpGet]
        public async Task<IActionResult> TaoPhieuMua(int id)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null) return NotFound();

            var phieuMuon = new PhieuMuon
            {
                MaSach = id,
                Sach = sach,
                NgayMuon = DateTime.Now,
                NgayHenTra = DateTime.Now,
                TrangThai = "Chờ xác nhận"
            };

            return View(phieuMuon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieuMua(int maSach, string ghiChu, int soLuong = 1)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

            var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
            if (docGia == null) return RedirectToAction("Login", "Account");

            var sach = await _context.Sachs.FindAsync(maSach);
            if (sach == null || sach.SoLuongCon <= 0)
            {
                TempData["Error"] = "Sách này hiện đã hết trong kho!";
                return RedirectToAction("Index", "Sach");
            }

            if (soLuong > sach.SoLuongCon)
            {
                soLuong = sach.SoLuongCon;
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = docGia.MaDocGia,
                MaSach = maSach,
                NgayMuon = DateTime.Now,
                NgayHenTra = DateTime.Now,
                TrangThai = "Chờ xác nhận",
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? "Đăng ký mua sách trực tuyến" : ghiChu.Trim()
            };

            _context.PhieuMuons.Add(phieuMuon);
            await _context.SaveChangesAsync();

            var chiTiet = new ChiTietPhieuMuon
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                MaSach = maSach,
                SoLuong = soLuong
            };
            _context.ChiTietPhieuMuons.Add(chiTiet);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(docGia.Email))
            {
                string bodyHtml = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ffc107; border-radius: 12px; max-width: 600px; margin: auto;'>
                        <h2 style='color: #d39e00; text-align: center; border-bottom: 2px solid #ffc107; padding-bottom: 10px;'>ĐÃ TIẾP NHẬN YÊU CẦU MUA SÁCH</h2>
                        <p>Xin chào <b>{docGia.HoTen}</b>,</p>
                        <p>Đơn hàng đặt mua sách đang <b><span style='color: #fd7e14;'>Chờ Quản trị viên duyệt</span></b>.</p>
                        <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Mã phiếu:</td><td style='padding: 8px;'>PM-{phieuMuon.MaPhieuMuon}</td></tr>
                            <tr style='background-color: #f8f9fa;'><td style='padding: 8px; font-weight: bold;'>Tên sách:</td><td style='padding: 8px;'>{sach.TenSach}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Số lượng mua:</td><td style='padding: 8px;'>{soLuong} cuốn</td></tr>
                        </table>
                    </div>";

                _ = EmailService.SendEmailAsync(docGia.Email, "[Trung Tâm Thư Viện] ĐÃ TIẾP NHẬN YÊU CẦU MUA SÁCH", bodyHtml);
            }

            TempData["Success"] = "Gửi yêu cầu mua sách thành công!";
            return RedirectToAction(nameof(LichSuMuon));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu(int id)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

            var phieu = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieu != null && (phieu.TrangThai == "Chờ xác nhận" || phieu.TrangThai == "Đang mượn"))
            {
                if (phieu.TrangThai == "Đang mượn")
                {
                    foreach (var item in phieu.ChiTietPhieuMuons)
                    {
                        var sach = await _context.Sachs.FindAsync(item.MaSach);
                        if (sach != null)
                        {
                            sach.SoLuongCon = Math.Min(sach.SoLuong, sach.SoLuongCon + item.SoLuong);
                        }
                    }
                }

                phieu.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã hủy đơn mượn/mua sách thành công!";
            }

            return RedirectToAction(nameof(LichSuMuon));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetPhieu(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var phieu = await _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieu != null && phieu.TrangThai == "Chờ xác nhận")
            {
                foreach (var item in phieu.ChiTietPhieuMuons)
                {
                    var sach = await _context.Sachs.FindAsync(item.MaSach);
                    if (sach == null || sach.SoLuongCon < item.SoLuong)
                    {
                        TempData["Error"] = "Sách không đủ số lượng trong kho!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var item in phieu.ChiTietPhieuMuons)
                {
                    var sach = await _context.Sachs.FindAsync(item.MaSach);
                    if (sach != null)
                    {
                        sach.SoLuongCon = Math.Max(0, sach.SoLuongCon - item.SoLuong);
                    }
                }

                bool isMua = phieu.GhiChu != null && phieu.GhiChu.ToLower().Contains("mua");
                phieu.TrangThai = isMua ? "Đã mua" : "Đang mượn";

                await _context.SaveChangesAsync();
                TempData["Success"] = "Duyệt đơn thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraSach(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon != null && phieuMuon.TrangThai == "Đang mượn")
            {
                phieuMuon.NgayTraThucTe = DateTime.Now;
                phieuMuon.TrangThai = "Đã trả";

                foreach (var item in phieuMuon.ChiTietPhieuMuons)
                {
                    var sach = await _context.Sachs.FindAsync(item.MaSach);
                    if (sach != null)
                    {
                        sach.SoLuongCon = Math.Min(sach.SoLuong, sach.SoLuongCon + item.SoLuong);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Xác nhận trả sách thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .FirstOrDefaultAsync(m => m.MaPhieuMuon == id);

            if (phieuMuon == null) return NotFound();

            return View(phieuMuon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null) return NotFound();

            bool isAdmin = IsAdmin();
            if (!isAdmin)
            {
                var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
                if (!maTaiKhoan.HasValue) return RedirectToAction("Login", "Account");

                var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
                if (docGia == null || phieuMuon.MaDocGia != docGia.MaDocGia)
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            if (phieuMuon.ChiTietPhieuMuons != null && phieuMuon.ChiTietPhieuMuons.Any())
            {
                _context.ChiTietPhieuMuons.RemoveRange(phieuMuon.ChiTietPhieuMuons);
            }

            _context.PhieuMuons.Remove(phieuMuon);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa phiếu mượn thành công!";
            return isAdmin ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(LichSuMuon));
        }
    }
}