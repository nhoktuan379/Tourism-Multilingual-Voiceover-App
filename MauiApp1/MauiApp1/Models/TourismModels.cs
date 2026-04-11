namespace MauiApp1.Models
{
    public class TourismSite
    {
        public int SiteID { get; set; }
        public string TourismName { get; set; } = null!;
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // ✅ THÊM DÒNG NÀY: Để chứa thông tin dịch từ bảng SiteTranslations
        public string? QuickInfo { get; set; }
    }

    // Các class khác giữ nguyên
    public class SiteAudio
    {
        public int AudioID { get; set; }
        public int SiteID { get; set; }
        public string AudioURL { get; set; } = null!;
        public string? VoiceGender { get; set; }
    }

    // Cập nhật Model Tour để khớp với DB Admin (Tours + TourTranslations)
    public class Tour
    {
        public int TourID { get; set; }
        public string TourName { get; set; } = null!;
        public string? CoverImage { get; set; } // Khớp với DB là CoverImage
        public decimal? TotalDistance { get; set; }
        public string? Duration { get; set; }

        // ✅ THÊM DÒNG NÀY: Để chứa mô tả tour theo ngôn ngữ
        public string? TourDescription { get; set; }
    }
}