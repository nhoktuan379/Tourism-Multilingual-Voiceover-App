using Microsoft.Maui.Maps;
using QRCoder;
using System.IO;
using ZXing.Net.Maui.Controls;
using BruTile.MbTiles; // Giải quyết lỗi MbTilesTileSource
using SQLite;          // Giải quyết lỗi SQLiteConnectionString
using Mapsui.Tiling.Layers;
using System.Globalization;
using MauiApp1.Resources.Languages;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            GenerateAndDisplayQR();

        }
        private void GenerateAndDisplayQR()
        {
            // Nội dung mã QR: Đây là đường dẫn (Route) đến NewPage1 đã khai báo trong AppShell
            string targetRoute = "page1";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(targetRoute, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // Hiển thị mảng byte ảnh lên Image control
                QrCodeImage.Source = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
            }
        }
        private async void OnScanButtonClicked(object sender, EventArgs e)
        {
            // Lệnh này sẽ đẩy người dùng sang trang ScanPage
            await Shell.Current.GoToAsync("ScanPage");
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
        public void ChangeLanguage(int langId)
        {
            string cultureCode = langId switch
            {
                1 => "vi-VN",
                2 => "en-US",
                3 => "ja-JP",
                _ => "vi-VN"
            };

            var culture = new CultureInfo(cultureCode);

            // Cập nhật cho hệ thống
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // Quan trọng: Cập nhật cho Resource Manager để UI nhận diện
            AppResources.Culture = culture;
            lblLogin.Text = AppResources.Login;
        }
        int currentLangId = 1;

        private void OnSelectVietnam(object sender, EventArgs e)
        {
            currentLangId = 1;
            CurrentLangImage.Source = "vietnam.png";
            LangMenu.IsVisible = false;
            ChangeLanguage(currentLangId);
        }

        private void OnSelectEnglish(object sender, EventArgs e)
        {
            currentLangId = 2;
            CurrentLangImage.Source = "united-kingdom.png";
            LangMenu.IsVisible = false;
            ChangeLanguage(currentLangId);
        }

        private void OnSelectJapanese(object sender, EventArgs e)
        {
            currentLangId = 3;
            CurrentLangImage.Source = "japan.png";
            LangMenu.IsVisible = false;
            ChangeLanguage(currentLangId);
        }
        bool isMenuOpen = false;
        private void OnToggleLanguageMenu(object sender, EventArgs e)
        {
            isMenuOpen = !isMenuOpen;
            LangMenu.IsVisible = isMenuOpen;
        }
    }

}
