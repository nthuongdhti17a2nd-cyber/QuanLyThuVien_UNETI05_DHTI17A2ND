// Họ và tên: Phạm Thị Thùy Linh
// Mã sinh viên: 23103100005
// Nội dung thực hiện: Xây dựng Controller Quản lý Tài khoản, Đăng nhập, Phân quyền hệ thống, Quản lý Thể loại sách và Dịch vụ gửi Email tự động.
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Services
{
    public class EmailService
    {
        private static readonly string _fromEmail = "trungtamthuviennhom5@gmail.com";
        private static readonly string _appPassword = "lojs hfix ypuz gmxv"; // Mã 16 ký tự Google App Password

        public static async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_fromEmail, _appPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "Trung Tâm Thư Viện - Nhóm 05"),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception)
            {
                // Bỏ qua ngoại lệ nếu email nhận không tồn tại để không gián đoạn app
            }
        }
    }
}