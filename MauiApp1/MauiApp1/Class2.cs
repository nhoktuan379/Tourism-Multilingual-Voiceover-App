using MauiApp1;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

public class SqlService
{
    // Thay đổi thông số kết nối của bạn ở đây
    // Chuỗi kết nối chuẩn cho máy ảo Android kết nối về SQL Server trên máy tính
    string connectionString = "Server=10.0.2.2;Database=TourismVoiceOverApp;User Id=sa;Password=123456;TrustServerCertificate=True;Encrypt=False;";



    public async Task<List<TourismSiteModel>> GetTourismSitesRawAsync()
    {
        var list = new List<TourismSiteModel>();

        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // Truy vấn trực tiếp từ bảng chính, lấy thêm Latitude và Longitude để làm GPS
                string sql = "SELECT SiteID, TourismName, Address, Latitude, Longitude FROM TourismSites";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new TourismSiteModel
                            {
                                SiteID = reader.GetInt32(0),
                                TourismName = reader.GetString(1),
                                Address = reader.GetString(2),
                                // Latitude và Longitude trong SQL là Decimal, nên dùng GetDouble hoặc GetDecimal
                                Latitude = (double)reader.GetDecimal(3),
                                Longitude = (double)reader.GetDecimal(4)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Hiển thị lỗi ra Alert để dễ debug trên mobile
            await App.Current.MainPage.DisplayAlert("Lỗi SQL", ex.Message, "OK");
        }

        return list;
    }

}
