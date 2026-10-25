// Họ và tên: Nguyễn Thị Hương
// Mã sinh viên: 23103100067
// Nội dung thực hiện: Xây dựng Controller Dashboard Quản trị, Phân hệ Quản lý mượn trả sách (Duyệt đơn, Trả sách, Hủy phiếu) và Thống kê báo cáo dữ liệu.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QUANLYTHUVIEN_UNETI_Nhom05_DHTI17A2ND.Data;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Data;
using System.Linq;

namespace QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang Bảng Quản Trị
        public IActionResult Index()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin" && HttpContext.Session.GetString("TenDangNhap") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // Trang Thống Kê & Báo Cáo Số Liệu
        public IActionResult ThongKeBaoCao()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin" && HttpContext.Session.GetString("TenDangNhap") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TongSoSach = _context.Sachs.Count();
            ViewBag.TongDocGia = _context.DocGias.Count();
            ViewBag.TongPhieuMuon = _context.PhieuMuons.Count();

            return View();
        }
    }
}