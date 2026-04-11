using Admin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages
{
    // Cấu trúc nhận dữ liệu từ Client gửi lên
    public class TranslationUpdateModel
    {
        public int SiteID { get; set; }
        public int LangID { get; set; }
        public string QuickInfo { get; set; }
    }

    // View Model để hiển thị dữ liệu ra giao diện
    public class SiteTranslationViewModel
    {
        public int SiteID { get; set; }
        public string? SiteName { get; set; }
        public string? OriginalInfo { get; set; }
        public List<SiteTranslation> Translations { get; set; } = new();
    }

    public class Index2Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        public Index2Model(ApplicationDbContext context) => _context = context;

        public List<SiteTranslationViewModel> SiteData { get; set; } = new();
        public List<Language> OtherLanguages { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1. Lấy danh sách ngôn ngữ dịch (bỏ Tiếng Việt ID=1)
            OtherLanguages = await _context.Languages.Where(l => l.LangID != 1).ToListAsync();

            // 2. Lấy toàn bộ dữ liệu để map
            var sites = await _context.TourismSites.ToListAsync();
            var allTrans = await _context.SiteTranslations.ToListAsync();

            foreach (var site in sites)
            {
                SiteData.Add(new SiteTranslationViewModel
                {
                    SiteID = site.SiteID,
                    SiteName = site.TourismName,
                    OriginalInfo = allTrans.FirstOrDefault(t => t.SiteID == site.SiteID && t.LangID == 1)?.QuickInfo ?? "Chưa có nội dung gốc",
                    Translations = allTrans.Where(t => t.SiteID == site.SiteID && t.LangID != 1).ToList()
                });
            }
        }

        // HÀM LƯU THỰC TẾ
        public async Task<IActionResult> OnPostTranslations([FromBody] List<TranslationUpdateModel> data)
        {
            if (data == null || !data.Any()) return new JsonResult(new { success = false, message = "Không có dữ liệu để lưu" });

            try
            {
                foreach (var item in data)
                {
                    // Tìm bản dịch đã tồn tại trong DB
                    var existingTrans = await _context.SiteTranslations
                        .FirstOrDefaultAsync(t => t.SiteID == item.SiteID && t.LangID == item.LangID);

                    if (existingTrans != null)
                    {
                        // Nếu đã có -> Cập nhật nội dung
                        existingTrans.QuickInfo = item.QuickInfo;
                    }
                    else if (!string.IsNullOrWhiteSpace(item.QuickInfo))
                    {
                        // Nếu chưa có và người dùng có nhập nội dung -> Thêm mới bản ghi
                        _context.SiteTranslations.Add(new SiteTranslation
                        {
                            SiteID = item.SiteID,
                            LangID = item.LangID,
                            QuickInfo = item.QuickInfo
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
        public class DeleteTranslationRequest
        {
            public int SiteId { get; set; }
            public int LangId { get; set; }
        }

        public async Task<IActionResult> OnPostDeleteTranslation([FromBody] DeleteTranslationRequest request)
        {
            try
            {
                var trans = await _context.SiteTranslations
                    .FirstOrDefaultAsync(t => t.SiteID == request.SiteId && t.LangID == request.LangId);

                if (trans != null)
                {
                    _context.SiteTranslations.Remove(trans);
                    await _context.SaveChangesAsync();
                    return new JsonResult(new { success = true });
                }

                return new JsonResult(new { success = false, message = "Không tìm thấy bản dịch để xóa." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class UpdateTranslationRequest
        {
            public int SiteId { get; set; }
            public int LangId { get; set; }
            public string? Content { get; set; }
        }

        public async Task<IActionResult> OnPostUpdateTranslation([FromBody] UpdateTranslationRequest request)
        {
            try
            {
                var existing = await _context.SiteTranslations
                    .FirstOrDefaultAsync(t => t.SiteID == request.SiteId && t.LangID == request.LangId);

                if (existing != null)
                {
                    // update
                    existing.QuickInfo = request.Content;
                }
                else
                {
                    // insert mới
                    _context.SiteTranslations.Add(new SiteTranslation
                    {
                        SiteID = request.SiteId,
                        LangID = request.LangId,
                        QuickInfo = request.Content ?? ""
                    });
                }

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }



}
