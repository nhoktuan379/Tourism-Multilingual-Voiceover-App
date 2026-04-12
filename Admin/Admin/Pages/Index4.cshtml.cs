using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Admin.Data;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages
{
    [IgnoreAntiforgeryToken]
    public class Index4Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Tour> ExistingTours { get; set; } = new();

        public Index4Model(ApplicationDbContext context) => _context = context;

        public async Task OnGetAsync()
        {
            // Lấy danh sách tour để hiện lên bảng
            ExistingTours = await _context.Tours.ToListAsync();
        }

        public class TourRequest
        {
            public string TourName { get; set; } = null!;
            public string? CoverImage { get; set; }
            public decimal? TotalDistance { get; set; }
            public string? Duration { get; set; }
            public List<int> PoiIds { get; set; } = new();
        }

        public async Task<IActionResult> OnPostSaveTour([FromBody] TourRequest data)
        {
            if (data == null || data.PoiIds.Count < 2)
                return new JsonResult(new { success = false, message = "Cần ít nhất 2 điểm" });

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var newTour = new Tour
                {
                    TourName = data.TourName,
                    CoverImage = data.CoverImage,
                    TotalDistance = data.TotalDistance,
                    Duration = data.Duration,
                    IsActive = true
                };
                _context.Tours.Add(newTour);
                await _context.SaveChangesAsync();

                for (int i = 0; i < data.PoiIds.Count; i++)
                {
                    _context.TourDetails.Add(new TourDetail
                    {
                        TourID = newTour.TourID,
                        SiteID = data.PoiIds[i],
                        OrderIndex = i + 1
                    });
                }
                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetTourDetails(int tourId)
        {
            var data = await _context.TourDetails
                .Where(td => td.TourID == tourId)
                .OrderBy(td => td.OrderIndex) // 🔥 QUAN TRỌNG
                .Join(_context.TourismSites,
                    td => td.SiteID,
                    s => s.SiteID,
                    (td, s) => new
                    {
                        id = s.SiteID,
                        name = s.TourismName,
                        lat = s.Latitude,
                        lng = s.Longitude,
                        order = td.OrderIndex
                    })
                .ToListAsync();

            return new JsonResult(data);
        }
    }

}
