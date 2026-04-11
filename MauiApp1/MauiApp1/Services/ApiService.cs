using System.Net.Http.Json;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class SqlService
    {
        // ⚠️ QUAN TRỌNG: Thay IP này bằng IP máy tính chạy Admin của bạn (ví dụ 192.168.1.40)
        // Và Port (ví dụ 5000) khớp với ASP.NET Admin
        private const string BaseUrl = "http://192.168.1.40:5000";
        private readonly HttpClient _httpClient;

        public SqlService()
        {
            _httpClient = new HttpClient();
        }

        // ✅ HÀM 1: Lấy chi tiết Site (Sửa lỗi CS1061 dòng 244)
        public async Task<TourismSite?> GetSiteByIdAsync(int siteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TourismSite>($"{BaseUrl}/Tourism/GetSite/{siteId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi API Site: {ex.Message}");
                return null;
            }
        }

        // ✅ HÀM 2: Lấy thông tin nhanh/bản dịch (Sửa lỗi CS1061 dòng 245)
        

        // ✅ HÀM 3: Lấy tất cả danh sách (Để vẽ Marker lúc bắt đầu)
        public async Task<List<TourismSite>> GetTourismSitesRawAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<TourismSite>>($"{BaseUrl}/Tourism/GetAllSites");
                return result ?? new List<TourismSite>();
            }
            catch
            {
                return new List<TourismSite>();
            }
        }

        // Thêm vào trong class SqlService
        public async Task<List<Tour>> GetToursBySiteIdAsync(int siteId)
        {
            try
            {
                // Giả sử API của bạn có endpoint này để lấy tour theo địa điểm
                var result = await _httpClient.GetFromJsonAsync<List<Tour>>($"{BaseUrl}/Tourism/GetToursBySite/{siteId}");
                return result ?? new List<Tour>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi API Tours: {ex.Message}");
                return new List<Tour>();
            }
        }

    }
}
