// Họ và tên: Phạm Thị Thùy Linh
// Mã sinh viên: 23103100005
// Nội dung thực hiện: Xây dựng Controller Quản lý Tài khoản, Đăng nhập, Phân quyền hệ thống, Quản lý Thể loại sách và Dịch vụ gửi Email tự động.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Data;
using QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Models;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Data;
using QuanLyThuVien_UNETI05_DHTI17A2ND.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN_UNETI_Nhom05__DHTI17A2ND.Controllers
{
    public class TheLoaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TheLoaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin";
        }

        // GET: TheLoai (Danh sách & Tìm kiếm)
        public async Task<IActionResult> Index(string searchString)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _context.TheLoais.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(t => t.TenTheLoai.Contains(searchString.Trim()));
            }

            ViewBag.SearchString = searchString;
            return View(await query.ToListAsync());
        }

        // GET: TheLoai/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: TheLoai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TheLoai theLoai)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(theLoai.TenTheLoai))
            {
                ModelState.AddModelError("TenTheLoai", "Tên thể loại không được để trống!");
            }
            else
            {
                string tenTrimmed = theLoai.TenTheLoai.Trim();
                // Kiểm tra trùng tên thể loại (Không phân biệt hoa thường)
                if (await _context.TheLoais.AnyAsync(t => t.TenTheLoai.ToLower() == tenTrimmed.ToLower()))
                {
                    ModelState.AddModelError("TenTheLoai", "Tên thể loại này đã tồn tại trong hệ thống!");
                }
                theLoai.TenTheLoai = tenTrimmed;
            }

            if (ModelState.IsValid)
            {
                _context.Add(theLoai);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm thể loại thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(theLoai);
        }

        // GET: TheLoai/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var theLoai = await _context.TheLoais.FindAsync(id);
            if (theLoai == null) return NotFound();

            return View(theLoai);
        }

        // POST: TheLoai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TheLoai theLoai)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != theLoai.MaTheLoai) return NotFound();

            if (string.IsNullOrWhiteSpace(theLoai.TenTheLoai))
            {
                ModelState.AddModelError("TenTheLoai", "Tên thể loại không được để trống!");
            }
            else
            {
                string tenTrimmed = theLoai.TenTheLoai.Trim();
                // Kiểm tra trùng tên khi Sửa (Trừ chính nó ra)
                if (await _context.TheLoais.AnyAsync(t => t.TenTheLoai.ToLower() == tenTrimmed.ToLower() && t.MaTheLoai != id))
                {
                    ModelState.AddModelError("TenTheLoai", "Tên thể loại này đã trùng với một thể loại khác!");
                }
                theLoai.TenTheLoai = tenTrimmed;
            }

            if (ModelState.IsValid)
            {
                _context.Update(theLoai);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thể loại thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(theLoai);
        }

        // GET: TheLoai/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var theLoai = await _context.TheLoais
                .Include(t => t.Sachs)
                .FirstOrDefaultAsync(m => m.MaTheLoai == id);

            if (theLoai == null) return NotFound();

            if (theLoai.Sachs != null && theLoai.Sachs.Any())
            {
                ViewBag.Warning = "Thể loại này đang chứa các cuốn sách trong hệ thống. Không thể xóa!";
            }

            return View(theLoai);
        }

        // POST: TheLoai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Chặn xóa nếu còn sách thuộc thể loại này trong CSDL
            bool coSach = await _context.Sachs.AnyAsync(s => s.MaTheLoai == id);
            if (coSach)
            {
                TempData["Error"] = "Không thể xóa! Thể loại này đang có sách liên kết trong thư viện.";
                return RedirectToAction(nameof(Index));
            }

            var theLoai = await _context.TheLoais.FindAsync(id);
            if (theLoai != null)
            {
                _context.TheLoais.Remove(theLoai);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa thể loại thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}