using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MauiApp1
{
    // Thêm public để dùng được ở các file khác
    public class TourismSiteModel
    {
        public int SiteID { get; set; }
        public string Version { get; set; } // Khớp với cột Version trong ảnh SQL của bạn
        public string TourismName { get; set; }
        public string Address { get; set; }

        // Tạm thời để double để sau này dùng cho tọa độ (Latitude, Longitude)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

