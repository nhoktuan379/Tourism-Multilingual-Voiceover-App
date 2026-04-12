using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Admin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TourismSite> TourismSites { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<SiteTranslation> SiteTranslations { get; set; }
        public DbSet<SiteAudio> SiteAudios { get; set; }
     
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourDetail> TourDetails { get; set; }
        public DbSet<TourTranslation> TourTranslations { get; set; }

        public DbSet<SiteImage> SiteImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Cấu hình khóa chính hỗn hợp
            modelBuilder.Entity<SiteTranslation>().HasKey(st => new { st.SiteID, st.LangID });
            

            // ✅ THÊM CẤU HÌNH KHÓA HỖN HỢP CHO TOUR:
            modelBuilder.Entity<TourDetail>().HasKey(td => new { td.TourID, td.OrderIndex });
            modelBuilder.Entity<TourTranslation>().HasKey(tt => new { tt.TourID, tt.LangID });

            // 2. ✅ FIX LỖI LÀM TRÒN TỌA ĐỘ: Cấu hình độ chính xác decimal (12 chữ số, 9 số sau dấu phẩy)
            modelBuilder.Entity<TourismSite>()
                .Property(p => p.Latitude)
                .HasPrecision(12, 9);

            modelBuilder.Entity<TourismSite>()
                .Property(p => p.Longitude)
                .HasPrecision(12, 9);
        }
    }

    [Table("TourismSites")]
    public class TourismSite
    {
        [Key]
        public int SiteID { get; set; }

        // ✅ THÊM DÒNG NÀY: Để khớp với code xử lý Version bạn viết ở trang Index.cshtml.cs
        public string? Version { get; set; }

        public string TourismName { get; set; } = null!;
        public string? Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        
    }

    [Table("Languages")]
    public class Language
    {
        [Key]
        public int LangID { get; set; }
        public string LangCode { get; set; } = null!;
        public string LangName { get; set; } = null!;
    }

    [Table("SiteTranslations")]
    public class SiteTranslation
    {
        public int SiteID { get; set; }
        public int LangID { get; set; }
        public string? QuickInfo { get; set; }
    }

    
    

    [Table("SiteAudios")]
    public class SiteAudio
    {
        [Key]
        public int AudioID { get; set; }
        public int SiteID { get; set; }
        public int LangID { get; set; }
        public string AudioURL { get; set; } = null!;
        public string? VoiceGender { get; set; }

    }

    [Table("Tours")]
    public class Tour
    {
        [Key]
        public int TourID { get; set; }
        public string TourName { get; set; } = null!;
        public string? CoverImage { get; set; }
        public decimal? TotalDistance { get; set; }
        public string? Duration { get; set; }
        public bool IsActive { get; set; } = true;
    }

    [Table("TourDetails")]
    public class TourDetail
    {
        // Khóa chính hỗn hợp (TourID + OrderIndex)
        public int TourID { get; set; }
        public int SiteID { get; set; }
        public int OrderIndex { get; set; }
    }

    [Table("TourTranslations")]
    public class TourTranslation
    {
        public int TourID { get; set; }
        public int LangID { get; set; }
        public string? TourDescription { get; set; }
    }

    [Table("SiteImages")]
    public class SiteImage
    {
        [Key]
        public int ImageID { get; set; }

        public int SiteID { get; set; }

        public string ImageURL { get; set; } = null!;

        public string ImageType { get; set; } = "Detail"; // Cover / Detail
    }

}
