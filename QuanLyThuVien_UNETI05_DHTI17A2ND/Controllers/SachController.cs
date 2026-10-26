// Họ và tên: Vũ Ngọc Dung
// Mã sinh viên: 23203100033
// Nội dung thực hiện: Xây dựng Controller Quản lý Sách, tích hợp tính năng Tìm kiếm, Lọc thể loại, Sắp xếp, Phân trang và Quản lý hình ảnh sách.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Data;
using QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Controllers
{
    public class SachController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SachController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("TenDangNhap"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin";
        }

        // GET: Sach (Showroom 30 sách/trang + Lọc + Sắp xếp + Tìm kiếm)
        public async Task<IActionResult> Index(string searchString, int? maTheLoai, string sortOrder, int pageNumber = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            ViewBag.SearchString = searchString;
            ViewBag.MaTheLoaiSelected = maTheLoai;
            ViewBag.CurrentSort = sortOrder;

            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.YearSortParm = sortOrder == "Year" ? "year_desc" : "Year";

            ViewBag.MaTheLoai = new SelectList(await _context.TheLoais.Where(t => t.TrangThai).ToListAsync(), "MaTheLoai", "TenTheLoai", maTheLoai);

            var query = _context.Sachs.Include(s => s.TheLoai).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string searchTrim = searchString.Trim();
                query = query.Where(s => s.TenSach.Contains(searchTrim) || s.TacGia.Contains(searchTrim));
            }

            if (maTheLoai.HasValue && maTheLoai.Value > 0)
            {
                query = query.Where(s => s.MaTheLoai == maTheLoai.Value);
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(s => s.TenSach),
                "Price" => query.OrderBy(s => s.GiaSach),
                "price_desc" => query.OrderByDescending(s => s.GiaSach),
                "Year" => query.OrderBy(s => s.NamXuatBan),
                "year_desc" => query.OrderByDescending(s => s.NamXuatBan),
                _ => query.OrderBy(s => s.TenSach),
            };

            int pageSize = 30;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        // GET: Sach/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var sach = await _context.Sachs
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(m => m.MaSach == id);

            if (sach == null) return NotFound();

            return View(sach);
        }

        // GET: Sach/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.MaTheLoai = new SelectList(await _context.TheLoais.Where(t => t.TrangThai).ToListAsync(), "MaTheLoai", "TenTheLoai");
            return View();
        }

        // POST: Sach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sach sach, IFormFile? fHinhAnh)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (sach.SoLuongCon > sach.SoLuong)
            {
                ModelState.AddModelError("SoLuongCon", "Số lượng còn không được vượt quá tổng số lượng sách.");
            }

            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh nếu Admin có chọn khi tạo mới
                if (fHinhAnh != null && fHinhAnh.Length > 0)
                {
                    string fileName = Path.GetFileName(fHinhAnh.FileName);
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/sach");

                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string filePath = Path.Combine(uploadDir, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fHinhAnh.CopyToAsync(fileStream);
                    }

                    sach.HinhAnh = fileName;
                }

                _context.Add(sach);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sách mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaTheLoai = new SelectList(await _context.TheLoais.Where(t => t.TrangThai).ToListAsync(), "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
            return View(sach);
        }

        // GET: Sach/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null) return NotFound();

            ViewBag.MaTheLoai = new SelectList(await _context.TheLoais.Where(t => t.TrangThai).ToListAsync(), "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
            return View(sach);
        }

        // POST: Sach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sach sach, IFormFile? fHinhAnh)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != sach.MaSach) return NotFound();

            if (sach.SoLuongCon > sach.SoLuong)
            {
                ModelState.AddModelError("SoLuongCon", "Số lượng còn không được vượt quá tổng số lượng sách.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ trong DB để giữ lại ảnh cũ nếu không chọn ảnh mới
                    var sachDb = await _context.Sachs.AsNoTracking().FirstOrDefaultAsync(s => s.MaSach == id);

                    if (fHinhAnh != null && fHinhAnh.Length > 0)
                    {
                        string fileName = Path.GetFileName(fHinhAnh.FileName);
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/sach");

                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        string filePath = Path.Combine(uploadDir, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await fHinhAnh.CopyToAsync(fileStream);
                        }

                        sach.HinhAnh = fileName;
                    }
                    else if (sachDb != null)
                    {
                        sach.HinhAnh = sachDb.HinhAnh;
                    }

                    _context.Update(sach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SachExists(sach.MaSach))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "Cập nhật thông tin sách thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaTheLoai = new SelectList(await _context.TheLoais.Where(t => t.TrangThai).ToListAsync(), "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
            return View(sach);
        }

        private bool SachExists(int id)
        {
            return _context.Sachs.Any(e => e.MaSach == id);
        }

        // GET: Sach/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var sach = await _context.Sachs
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(m => m.MaSach == id);
            if (sach == null) return NotFound();

            return View(sach);
        }

        // POST: Sach/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            bool coChiTiet = await _context.ChiTietPhieuMuons.AnyAsync(c => c.MaSach == id);
            if (coChiTiet)
            {
                TempData["Error"] = "Không thể xóa! Cuốn sách này đã từng có giao dịch mượn/mua trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            var sach = await _context.Sachs.FindAsync(id);
            if (sach != null)
            {
                _context.Sachs.Remove(sach);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa sách thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}