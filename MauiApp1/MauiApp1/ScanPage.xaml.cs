
using ZXing.Net.Maui;

namespace MauiApp1;

public partial class ScanPage : ContentPage
{
    public ScanPage()
    {
        InitializeComponent();

    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var firstBarcode = e.Results.FirstOrDefault();
        if (firstBarcode != null)
        {
            Dispatcher.Dispatch(async () =>
            {
                // Giả sử mã QR chứa chữ "page1"
                string code = firstBarcode.Value;

                // Tự động chuyển trang dựa trên nội dung mã QR
                await Shell.Current.GoToAsync($"///{code}");
            });
        }
    }
}


