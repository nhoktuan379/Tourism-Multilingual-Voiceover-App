using Admin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        // ✅ PHẢI thêm ApplicationDbContext vào tham số ở đây
        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Bây giờ context đã được truyền vào từ Program.cs
        }

        public List<TourismSite> Sites { get; set; } = new();

        // ✅ CHỈ DÙNG 1 hàm OnGetAsync, xóa hàm OnGet() cũ đi
        public async Task OnGetAsync()
        {
            // Lấy toàn bộ dữ liệu từ bảng TourismSites trong SQL
            Sites = await _context.TourismSites.ToListAsync();
        }

        public async Task<IActionResult> OnPostSavePoiAsync([FromBody] TourismSite newSite)
        {
            if (newSite == null || string.IsNullOrEmpty(newSite.TourismName))
                return new BadRequestObjectResult("Dữ liệu không hợp lệ");

            _context.TourismSites.Add(newSite);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        // Thêm hàm này vào file Index.cshtml.cs
        public async Task<IActionResult> OnPostDeletePoiAsync(int id)
        {
            // 1. Tìm địa điểm trong Database theo ID
            var siteToDelete = await _context.TourismSites.FindAsync(id);

            if (siteToDelete == null)
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy địa điểm để xóa!" });
            }

            try
            {
                // 2. Thực hiện xóa
                _context.TourismSites.Remove(siteToDelete);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }


    }
}


