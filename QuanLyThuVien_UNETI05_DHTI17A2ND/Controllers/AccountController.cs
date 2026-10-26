using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Data;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Controllers
{
    
        public class AccountController : Controller
        {
            private readonly ApplicationDbContext _context;

            public AccountController(ApplicationDbContext context)
            {
                _context = context;
            }

            // HÀM BĂM MẬT KHẨU BẰNG SHA256 BẢO MẬT
            private string HashPassword(string password)
            {
                if (string.IsNullOrEmpty(password)) return string.Empty;
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var builder = new StringBuilder();
                    foreach (var b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }

            // GET: Account/Login
            public IActionResult Login() => View();

            [HttpPost]
            public IActionResult Login(string tenDangNhap, string matKhau)
            {
                if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                    return View();
                }

                tenDangNhap = tenDangNhap.Trim();
                string matKhauBam = HashPassword(matKhau.Trim());

                // 1. Kiểm tra tài khoản, mật khẩu băm và kiểm tra trạng thái hoạt động của tài khoản
                var tk = _context.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == tenDangNhap &&
                            (t.MatKhau == matKhauBam || t.MatKhau == matKhau.Trim()));

                if (tk == null)
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                    return View();
                }

                // Kiểm tra nếu tài khoản bị khóa (TrangThai == false)
                if (!tk.TrangThai)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa! Vui lòng liên hệ Admin.";
                    return View();
                }

                // 2. Tìm thông tin Độc giả theo MaTaiKhoan
                var docGia = _context.DocGias.FirstOrDefault(d => d.MaTaiKhoan == tk.MaTaiKhoan);

                // 3. Lưu thông tin vào Session
                HttpContext.Session.SetString("TenDangNhap", tk.TenDangNhap);
                HttpContext.Session.SetString("VaiTro", tk.VaiTro ?? "DocGia");
                HttpContext.Session.SetInt32("MaTaiKhoan", tk.MaTaiKhoan);

                if (docGia != null)
                {
                    HttpContext.Session.SetInt32("MaDocGia", docGia.MaDocGia);
                }

                // 4. PHÂN QUYỀN ĐIỀU HƯỚNG TỰ ĐỘNG
                if (string.Equals(tk.VaiTro, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            public IActionResult Logout()
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            // --- ĐĂNG KÝ TÀI KHOẢN ĐỘC GIẢ ---
            public IActionResult Register() => View();

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Register(string tenDangNhap, string matKhau, string hoTen, string email, string soDienThoai)
            {
                if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                {
                    ViewBag.Error = "Tên đăng nhập và mật khẩu không được để trống!";
                    return View();
                }

                tenDangNhap = tenDangNhap.Trim();

                if (_context.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap))
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại trong hệ thống!";
                    return View();
                }

                try
                {
                    var tk = new TaiKhoan
                    {
                        TenDangNhap = tenDangNhap,
                        MatKhau = HashPassword(matKhau.Trim()), // Băm SHA256
                        VaiTro = "DocGia",
                        TrangThai = true
                    };
                    _context.TaiKhoans.Add(tk);
                    await _context.SaveChangesAsync();

                    var docGia = new DocGia
                    {
                        MaTaiKhoan = tk.MaTaiKhoan,
                        HoTen = hoTen?.Trim() ?? "Độc giả mới",
                        Email = email?.Trim(),
                        SoDienThoai = soDienThoai?.Trim(),
                        DiaChi = "Chưa cập nhật"
                    };

                    _context.DocGias.Add(docGia);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ViewBag.Error = "Lỗi khi lưu dữ liệu: " + innerMessage;
                    return View();
                }
            }

            // --- QUÊN MẬT KHẨU (GỬI MÃ OTP QUA EMAIL TỰ ĐỘNG) ---
            public IActionResult ForgotPassword() => View();

            [HttpPost]
            public async Task<IActionResult> SendResetCode(string tenDangNhap)
            {
                if (string.IsNullOrWhiteSpace(tenDangNhap))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên đăng nhập!" });
                }

                var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap.Trim());
                if (tk == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại!" });
                }

                var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == tk.MaTaiKhoan);
                if (docGia == null || string.IsNullOrWhiteSpace(docGia.Email))
                {
                    return Json(new { success = false, message = "Tài khoản chưa được đăng ký Email để nhận mã OTP!" });
                }

                // Tạo mã OTP ngẫu nhiên 6 chữ số
                Random random = new Random();
                string otpCode = random.Next(100000, 999999).ToString();

                // Lưu OTP vào Session
                HttpContext.Session.SetString("ResetCode", otpCode);
                HttpContext.Session.SetString("ResetUser", tk.TenDangNhap);

                // Gửi Email OTP cho độc giả
                string bodyHtml = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #0d6efd; border-radius: 12px; max-width: 500px; margin: auto;'>
                    <h2 style='color: #0d6efd; text-align: center;'>MÃ XÁC THỰC KHÔI PHỤC MẬT KHẨU</h2>
                    <p>Xin chào <b>{docGia.HoTen}</b>,</p>
                    <p>Mã OTP để đặt lại mật khẩu cho tài khoản <b>{tk.TenDangNhap}</b> tại Trung Tâm Thư Viện là:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='background-color: #f8f9fa; border: 2px dashed #0d6efd; color: #dc3545; font-size: 32px; font-weight: bold; padding: 10px 20px; letter-spacing: 5px; border-radius: 8px;'>{otpCode}</span>
                    </div>
                    <p style='color: #6c757d; font-size: 13px;'>Vui lòng không chia sẻ mã này cho bất kỳ ai để bảo mật tài khoản.</p>
                </div>";

                _ = EmailService.SendEmailAsync(docGia.Email, "[Trung Tâm Thư Viện] Mã OTP Khôi phục mật khẩu", bodyHtml);

                return Json(new { success = true, message = $"Mã OTP đã được gửi đến email ({docGia.Email}). Vui lòng kiểm tra hộp thư!" });
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult ForgotPassword(string tenDangNhap, string maXacThuc, string matKhauMoi)
            {
                var sessionCode = HttpContext.Session.GetString("ResetCode");
                var sessionUser = HttpContext.Session.GetString("ResetUser");

                if (string.IsNullOrWhiteSpace(maXacThuc) || string.IsNullOrWhiteSpace(sessionCode) ||
                    maXacThuc.Trim().ToUpper() != sessionCode.Trim().ToUpper() ||
                    tenDangNhap?.Trim() != sessionUser)
                {
                    ViewBag.Error = "Mã xác thực OTP không chính xác hoặc đã hết hạn!";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(matKhauMoi))
                {
                    ViewBag.Error = "Vui lòng nhập mật khẩu mới!";
                    return View();
                }

                var tk = _context.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == tenDangNhap.Trim());
                if (tk == null)
                {
                    ViewBag.Error = "Tài khoản không tồn tại!";
                    return View();
                }

                tk.MatKhau = HashPassword(matKhauMoi.Trim()); // Băm mật khẩu mới bằng SHA256
                _context.TaiKhoans.Update(tk);
                _context.SaveChanges();

                // Xóa mã reset code trong session
                HttpContext.Session.Remove("ResetCode");
                HttpContext.Session.Remove("ResetUser");

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }
        }
    }


