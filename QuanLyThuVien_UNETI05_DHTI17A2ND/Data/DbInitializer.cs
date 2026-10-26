// Họ và tên: Nguyễn Trung Kiên
// Mã sinh viên: 23274800010
// Nội dung thực hiện: Khởi tạo dữ liệu mẫu (Seed Data) - 30 đầu sách và tự động sinh danh sách độc giả từ docgia4 đến docgia24

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;

namespace QuanLyThuVien_UNETI05_DHTI17A2ND.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Thêm Thể loại mẫu nếu chưa có
            if (!context.TheLoais.Any())
            {
                var theLoais = new TheLoai[]
                {
                    new TheLoai { TenTheLoai = "Công nghệ thông tin", MoTa = "Sách lập trình, phần mềm, CSDL", TrangThai = true },
                    new TheLoai { TenTheLoai = "Kinh tế - Quản trị", MoTa = "Sách kinh doanh, quản trị doanh nghiệp", TrangThai = true },
                    new TheLoai { TenTheLoai = "Văn học Việt Nam", MoTa = "Tiểu thuyết, truyện ngắn Việt Nam", TrangThai = true },
                    new TheLoai { TenTheLoai = "Ngoại ngữ", MoTa = "Sách học Tiếng Anh, Tiếng Trung, Tiếng Nhật", TrangThai = true },
                    new TheLoai { TenTheLoai = "Kỹ năng sống", MoTa = "Sách phát triển bản thân", TrangThai = true }
                };
                context.TheLoais.AddRange(theLoais);
                context.SaveChanges();
            }

            // =========================================================================
            // ĐOẠN CODE SQL TỰ ĐỘNG CẬP NHẬT LẠI MÃ THỂ LOẠI CHUẨN XÁC MỖI KHI CHẠY APP
            // =========================================================================
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    UPDATE Sachs SET MaTheLoai = (SELECT TOP 1 MaTheLoai FROM TheLoais WHERE TenTheLoai = N'Công nghệ thông tin') WHERE TenSach LIKE N'%Lập trình%' OR TenSach LIKE N'%ASP.NET%' OR TenSach LIKE N'%Cấu trúc dữ liệu%' OR TenSach LIKE N'%Trí tuệ nhân tạo%' OR TenSach LIKE N'%SQL Server%' OR TenSach LIKE N'%ReactJS%' OR TenSach LIKE N'%Bảo mật%';
                    UPDATE Sachs SET MaTheLoai = (SELECT TOP 1 MaTheLoai FROM TheLoais WHERE TenTheLoai = N'Kinh tế - Quản trị') WHERE TenSach LIKE N'%Nhà Giả Kim%' OR TenSach LIKE N'%Cha giàu%' OR TenSach LIKE N'%Tư duy nhanh%' OR TenSach LIKE N'%Khởi nghiệp%' OR TenSach LIKE N'%Chiến lược đại dương xanh%';
                    UPDATE Sachs SET MaTheLoai = (SELECT TOP 1 MaTheLoai FROM TheLoais WHERE TenTheLoai = N'Kỹ năng sống') WHERE TenSach LIKE N'%Đắc Nhân Tâm%' OR TenSach LIKE N'%thói quen%' OR TenSach LIKE N'%Đời thay đổi%' OR TenSach LIKE N'%Quản lý thời gian%' OR TenSach LIKE N'%Trí tuệ cảm xúc%' OR TenSach LIKE N'%Tuổi trẻ đáng giá%';
                    UPDATE Sachs SET MaTheLoai = (SELECT TOP 1 MaTheLoai FROM TheLoais WHERE TenTheLoai = N'Văn học Việt Nam') WHERE TenSach LIKE N'%Mắt Biếc%' OR TenSach LIKE N'%vé đi tuổi thơ%' OR TenSach LIKE N'%Số Đỏ%' OR TenSach LIKE N'%Dế Mèn%' OR TenSach LIKE N'%Harry Potter%';
                    UPDATE Sachs SET MaTheLoai = (SELECT TOP 1 MaTheLoai FROM TheLoais WHERE TenTheLoai = N'Ngoại ngữ') WHERE TenSach LIKE N'%English%' OR TenSach LIKE N'%Hack Não%' OR TenSach LIKE N'%Minna%' OR TenSach LIKE N'%HSK%' OR TenSach LIKE N'%Tiếng Hàn%' OR TenSach LIKE N'%IELTS%' OR TenSach LIKE N'%Toeic%';
                ");
            }
            catch { /* Bỏ qua nếu bảng chưa sẵn sàng */ }

            // 2. Thêm Tài khoản Admin & Độc giả mẫu
            if (!context.TaiKhoans.Any(t => t.TenDangNhap == "admin"))
            {
                var adminTk = new TaiKhoan
                {
                    TenDangNhap = "admin",
                    MatKhau = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", // 123456 băm SHA256
                    HoTen = "Quản trị viên",
                    VaiTro = "Admin",
                    TrangThai = true
                };
                context.TaiKhoans.Add(adminTk);
                context.SaveChanges();
            }

            // 3. Vòng lặp tự động tạo các tài khoản từ docgia1 đến docgia24
            for (int i = 1; i <= 24; i++)
            {
                string username = $"docgia{i}";
                var existingTk = context.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == username);
                if (existingTk == null)
                {
                    var newTk = new TaiKhoan
                    {
                        TenDangNhap = username,
                        MatKhau = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92",
                        HoTen = $"Độc Giả Số {i}",
                        VaiTro = "DocGia",
                        TrangThai = true
                    };
                    context.TaiKhoans.Add(newTk);
                    context.SaveChanges();

                    var newDocGia = new DocGia
                    {
                        MaTaiKhoan = newTk.MaTaiKhoan,
                        HoTen = $"Độc Giả Số {i}",
                        NgaySinh = new DateTime(2003, 1, 1).AddDays(i * 15),
                        GioiTinh = (i % 2 == 0) ? "Nữ" : "Nam",
                        SoDienThoai = $"0987654{i.ToString("D3")}",
                        Email = $"docgia{i}@gmail.com",
                        DiaChi = $"Hà Nội - Khu vực {i}",
                        NgayDangKy = DateTime.Now,
                        TrangThai = true
                    };
                    context.DocGias.Add(newDocGia);
                    context.SaveChanges();
                }
            }

            // 4. Danh sách 30 đầu sách chuẩn kèm ảnh bìa
            var cnttId = context.TheLoais.First(t => t.TenTheLoai == "Công nghệ thông tin").MaTheLoai;
            var kinhTeId = context.TheLoais.First(t => t.TenTheLoai == "Kinh tế - Quản trị").MaTheLoai;
            var vanHocId = context.TheLoais.First(t => t.TenTheLoai == "Văn học Việt Nam").MaTheLoai;
            var ngoaiNguId = context.TheLoais.First(t => t.TenTheLoai == "Ngoại ngữ").MaTheLoai;
            var knsId = context.TheLoais.First(t => t.TenTheLoai == "Kỹ năng sống").MaTheLoai;

            var danhSachSeed = new Sach[]
            {
                new Sach { TenSach = "Lập trình C# cơ bản đến nâng cao", MaTheLoai = cnttId, TacGia = "Nguyễn Văn B", NhaXuatBan = "NXB Giáo Dục", NamXuatBan = 2024, SoLuong = 10, SoLuongCon = 10, GiaBia = 120000, GiaSach = 120000, HinhAnh = "https://picsum.photos/id/1/500/300", MoTa = "Giáo trình C# đầy đủ", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Kỹ thuật lập trình ASP.NET Core MVC", MaTheLoai = cnttId, TacGia = "Trần Văn C", NhaXuatBan = "NXB Thông tin", NamXuatBan = 2025, SoLuong = 15, SoLuongCon = 15, GiaBia = 150000, GiaSach = 150000, HinhAnh = "https://picsum.photos/id/2/500/300", MoTa = "Hướng dẫn xây dựng Web MVC", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Cấu trúc dữ liệu và giải thuật", MaTheLoai = cnttId, TacGia = "Phạm Huy Hoàng", NhaXuatBan = "NXB Khoa Học", NamXuatBan = 2023, SoLuong = 25, SoLuongCon = 25, GiaBia = 110000, GiaSach = 110000, HinhAnh = "https://picsum.photos/id/3/500/300", MoTa = "Nền tảng cốt lõi cho sinh viên CNTT.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Nhập môn Trí tuệ nhân tạo (AI)", MaTheLoai = cnttId, TacGia = "John McCarthy", NhaXuatBan = "NXB Đại học Quốc gia", NamXuatBan = 2025, SoLuong = 10, SoLuongCon = 10, GiaBia = 200000, GiaSach = 200000, HinhAnh = "https://picsum.photos/id/4/500/300", MoTa = "Khám phá thế giới AI và Machine Learning.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Cơ sở dữ liệu SQL Server & EF Core", MaTheLoai = cnttId, TacGia = "Lê Thị Hồng", NhaXuatBan = "NXB Thống Kê", NamXuatBan = 2024, SoLuong = 18, SoLuongCon = 18, GiaBia = 135000, GiaSach = 135000, HinhAnh = "https://picsum.photos/id/5/500/300", MoTa = "Thiết kế CSDL và thao tác qua Entity Framework Core.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Lập trình Web Frontend với ReactJS", MaTheLoai = cnttId, TacGia = "Dan Abramov", NhaXuatBan = "NXB Lao Động", NamXuatBan = 2024, SoLuong = 12, SoLuongCon = 12, GiaBia = 160000, GiaSach = 160000, HinhAnh = "https://picsum.photos/id/6/500/300", MoTa = "Xây dựng giao diện web hiện đại với React.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Bảo mật ứng dụng Web và Network", MaTheLoai = cnttId, TacGia = "Kevin Mitnick", NhaXuatBan = "NXB Công An", NamXuatBan = 2023, SoLuong = 8, SoLuongCon = 8, GiaBia = 180000, GiaSach = 180000, HinhAnh = "https://picsum.photos/id/7/500/300", MoTa = "Các kỹ thuật phòng thủ và tấn công mạng cơ bản.", TrangThai = "Đang phục vụ" },

                new Sach { TenSach = "Nhà Giả Kim", MaTheLoai = kinhTeId, TacGia = "Paulo Coelho", NhaXuatBan = "NXB Văn Học", NamXuatBan = 2021, SoLuong = 25, SoLuongCon = 25, GiaBia = 79000, GiaSach = 79000, HinhAnh = "https://picsum.photos/id/8/500/300", MoTa = "Hành trình theo đuổi ước mơ.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Cha giàu cha nghèo (Tập 1)", MaTheLoai = kinhTeId, TacGia = "Robert Kiyosaki", NhaXuatBan = "NXB Trẻ", NamXuatBan = 2023, SoLuong = 22, SoLuongCon = 22, GiaBia = 95000, GiaSach = 95000, HinhAnh = "https://picsum.photos/id/9/500/300", MoTa = "Tư duy tài chính và quản lý tiền bạc.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Tư duy nhanh và chậm", MaTheLoai = kinhTeId, TacGia = "Daniel Kahneman", NhaXuatBan = "NXB Thế Giới", NamXuatBan = 2022, SoLuong = 14, SoLuongCon = 14, GiaBia = 155000, GiaSach = 155000, HinhAnh = "https://picsum.photos/id/10/500/300", MoTa = "Hiểu cách bộ não đưa ra quyết định kinh tế.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Khởi nghiệp tinh gọn", MaTheLoai = kinhTeId, TacGia = "Eric Ries", NhaXuatBan = "NXB Công Thương", NamXuatBan = 2023, SoLuong = 16, SoLuongCon = 16, GiaBia = 130000, GiaSach = 130000, HinhAnh = "https://picsum.photos/id/20/500/300", MoTa = "Phương pháp quản trị dự án khởi nghiệp thành công.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Chiến lược đại dương xanh", MaTheLoai = kinhTeId, TacGia = "W. Chan Kim", NhaXuatBan = "NXB Tổng Hợp TPHCM", NamXuatBan = 2021, SoLuong = 10, SoLuongCon = 10, GiaBia = 140000, GiaSach = 140000, HinhAnh = "https://picsum.photos/id/24/500/300", MoTa = "Tạo ra không gian thị trường không có đối thủ cạnh tranh.", TrangThai = "Đang phục vụ" },

                new Sach { TenSach = "Đắc Nhân Tâm", MaTheLoai = knsId, TacGia = "Dale Carnegie", NhaXuatBan = "NXB Trẻ", NamXuatBan = 2022, SoLuong = 30, SoLuongCon = 30, GiaBia = 90000, GiaSach = 90000, HinhAnh = "https://picsum.photos/id/26/500/300", MoTa = "Sách kỹ năng sống kinh điển", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Sức mạnh của thói quen", MaTheLoai = knsId, TacGia = "Charles Duhigg", NhaXuatBan = "NXB Lao Động", NamXuatBan = 2022, SoLuong = 18, SoLuongCon = 18, GiaBia = 109000, GiaSach = 109000, HinhAnh = "https://picsum.photos/id/28/500/300", MoTa = "Tại sao chúng ta làm những gì mình làm trong đời sống và kinh doanh.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Đời thay đổi khi chúng ta thay đổi", MaTheLoai = knsId, TacGia = "Andrew Matthews", NhaXuatBan = "NXB Tổng Hợp", NamXuatBan = 2020, SoLuong = 20, SoLuongCon = 20, GiaBia = 75000, GiaSach = 75000, HinhAnh = "https://picsum.photos/id/29/500/300", MoTa = "Góc nhìn tích cực để vượt qua mọi nghịch cảnh.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Quản lý thời gian xuất sắc", MaTheLoai = knsId, TacGia = "Brian Tracy", NhaXuatBan = "NXB Thanh Niên", NamXuatBan = 2023, SoLuong = 15, SoLuongCon = 15, GiaBia = 88000, GiaSach = 88000, HinhAnh = "https://picsum.photos/id/30/500/300", MoTa = "Làm ít hơn nhưng đạt hiệu suất cao hơn.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Trí tuệ cảm xúc (EQ)", MaTheLoai = knsId, TacGia = "Daniel Goleman", NhaXuatBan = "NXB Lao Động Xã Hội", NamXuatBan = 2022, SoLuong = 12, SoLuongCon = 12, GiaBia = 115000, GiaSach = 115000, HinhAnh = "https://picsum.photos/id/36/500/300", MoTa = "EQ quan trọng hơn IQ như thế nào trong công việc.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Tuổi trẻ đáng giá bao nhiêu", MaTheLoai = knsId, TacGia = "Rosie Nguyễn", NhaXuatBan = "NXB Hội Nhà Văn", NamXuatBan = 2020, SoLuong = 28, SoLuongCon = 28, GiaBia = 80000, GiaSach = 80000, HinhAnh = "https://picsum.photos/id/42/500/300", MoTa = "Cuốn sách gối đầu giường cho thế hệ trẻ.", TrangThai = "Đang phục vụ" },

                new Sach { TenSach = "Mắt Biếc", MaTheLoai = vanHocId, TacGia = "Nguyễn Nhật Ánh", NhaXuatBan = "NXB Trẻ", NamXuatBan = 2019, SoLuong = 25, SoLuongCon = 25, GiaBia = 90000, GiaSach = 90000, HinhAnh = "https://picsum.photos/id/48/500/300", MoTa = "Mối tình si sâu nặng của Ngạn dành cho Hà Mây.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Cho tôi xin một vé đi tuổi thơ", MaTheLoai = vanHocId, TacGia = "Nguyễn Nhật Ánh", NhaXuatBan = "NXB Trẻ", NamXuatBan = 2018, SoLuong = 20, SoLuongCon = 20, GiaBia = 85000, GiaSach = 85000, HinhAnh = "https://picsum.photos/id/60/500/300", MoTa = "Chuyến tàu quay về khoảng trời thơ ấu ngây thơ.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Số Đỏ", MaTheLoai = vanHocId, TacGia = "Vũ Trọng Phụng", NhaXuatBan = "NXB Văn Học", NamXuatBan = 2020, SoLuong = 15, SoLuongCon = 15, GiaBia = 70000, GiaSach = 70000, HinhAnh = "https://picsum.photos/id/64/500/300", MoTa = "Kiệt tác trào phúng nổi tiếng của văn học Việt Nam.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Dế Mèn phiêu lưu ký", MaTheLoai = vanHocId, TacGia = "Tô Hoài", NhaXuatBan = "NXB Kim Đồng", NamXuatBan = 2021, SoLuong = 30, SoLuongCon = 30, GiaBia = 65000, GiaSach = 65000, HinhAnh = "https://picsum.photos/id/65/500/300", MoTa = "Hành trình khám phá thế giới của chú dế cường tráng.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Harry Potter và Hòn đá Phù thủy", MaTheLoai = vanHocId, TacGia = "J.K. Rowling", NhaXuatBan = "NXB Trẻ", NamXuatBan = 2022, SoLuong = 18, SoLuongCon = 18, GiaBia = 195000, GiaSach = 195000, HinhAnh = "https://picsum.photos/id/107/500/300", MoTa = "Bước chân vào thế giới phép thuật huyền bí.", TrangThai = "Đang phục vụ" },

                new Sach { TenSach = "English Grammar in Use", MaTheLoai = ngoaiNguId, TacGia = "Raymond Murphy", NhaXuatBan = "NXB Đại học Cambridge", NamXuatBan = 2024, SoLuong = 35, SoLuongCon = 35, GiaBia = 165000, GiaSach = 165000, HinhAnh = "https://picsum.photos/id/119/500/300", MoTa = "Bộ sách ngữ pháp tiếng Anh kinh điển thế giới.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Hack Não 1500 Từ Vựng Tiếng Anh", MaTheLoai = ngoaiNguId, TacGia = "Lệnh Hồ Công", NhaXuatBan = "NXB Thế Giới", NamXuatBan = 2023, SoLuong = 25, SoLuongCon = 25, GiaBia = 220000, GiaSach = 220000, HinhAnh = "https://picsum.photos/id/133/500/300", MoTa = "Học từ vựng nhanh qua phương pháp âm thanh tương tự.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Minna no Nihongo Sơ cấp 1", MaTheLoai = ngoaiNguId, TacGia = "3A Corporation", NhaXuatBan = "NXB Dân Trí", NamXuatBan = 2022, SoLuong = 20, SoLuongCon = 20, GiaBia = 145000, GiaSach = 145000, HinhAnh = "https://picsum.photos/id/145/500/300", MoTa = "Giáo trình tiêu chuẩn học tiếng Nhật căn bản.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Giáo trình HSK 3 Tiếng Trung", MaTheLoai = ngoaiNguId, TacGia = "Đại học Ngôn ngữ Bắc Kinh", NhaXuatBan = "NXB Đại học Quốc gia", NamXuatBan = 2023, SoLuong = 15, SoLuongCon = 15, GiaBia = 135000, GiaSach = 135000, HinhAnh = "https://picsum.photos/id/160/500/300", MoTa = "Luyện thi chứng chỉ HSK trung cấp tiếng Trung.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Tự học Tiếng Hàn giao tiếp", MaTheLoai = ngoaiNguId, TacGia = "The Choi", NhaXuatBan = "NXB Hồng Đức", NamXuatBan = 2022, SoLuong = 12, SoLuongCon = 12, GiaBia = 125000, GiaSach = 125000, HinhAnh = "https://picsum.photos/id/175/500/300", MoTa = "Giao tiếp tiếng Hàn cấp tốc cho người mới bắt đầu.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "IELTS Cambridge Practice Tests 18", MaTheLoai = ngoaiNguId, TacGia = "Cambridge University", NhaXuatBan = "NXB Cambridge", NamXuatBan = 2024, SoLuong = 20, SoLuongCon = 20, GiaBia = 185000, GiaSach = 185000, HinhAnh = "https://picsum.photos/id/195/500/300", MoTa = "Sách luyện đề thi IELTS chuẩn quốc tế mới nhất.", TrangThai = "Đang phục vụ" },
                new Sach { TenSach = "Toeic 450+ Target", MaTheLoai = ngoaiNguId, TacGia = "Anne Taylor", NhaXuatBan = "NXB Tổng Hợp", NamXuatBan = 2023, SoLuong = 18, SoLuongCon = 18, GiaBia = 150000, GiaSach = 150000, HinhAnh = "https://picsum.photos/id/201/500/300", MoTa = "Luyện thi chứng chỉ TOEIC mục tiêu căn bản.", TrangThai = "Đang phục vụ" }
            };

            // 5. Tự động đồng bộ sách và ảnh
            foreach (var item in danhSachSeed)
            {
                var existingBook = context.Sachs.FirstOrDefault(s => s.TenSach == item.TenSach);
                if (existingBook == null)
                {
                    context.Sachs.Add(item);
                }
                else
                {
                    existingBook.HinhAnh = item.HinhAnh;
                    existingBook.MaTheLoai = item.MaTheLoai;
                    existingBook.TacGia = item.TacGia;
                    existingBook.GiaSach = item.GiaSach;
                    context.Update(existingBook);
                }
            }
            context.SaveChanges();
        }
    }
}