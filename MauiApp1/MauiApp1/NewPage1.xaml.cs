using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using System.Globalization;
using MauiApp1.Services;
using MauiApp1.Models;
using System.Text.Json;
using Mapsui.Layers;
using Mapsui.Tiling;
using Mapsui.UI.Maui;       
using Microsoft.Data.Sqlite;
using BruTile;
using Mapsui.Tiling.Layers;
using BruTile.MbTiles;
using Mapsui.UI;
using SQLite;
using System.IO;
using System.Globalization;
using MauiApp1.Resources.Languages;



namespace MauiApp1;

public partial class NewPage1 : ContentPage
{
    private CancellationTokenSource _cts;
    private bool _isTrackingStarted = false;

    private readonly MauiApp1.Services.SqlService _sqlService = new MauiApp1.Services.SqlService();

    public NewPage1()
    {
        InitializeComponent();

#if ANDROID
        // ✅ DÙNG IP THẬT (máy bạn)
        GoongMapView.Source = "http://10.107.159.25:5500/goong_map.html";
#else
        GoongMapView.Source = "http://localhost:5500/goong_map.html";
#endif

        ConfigureWebView();
        GoongMapView.Navigating += OnGoongMapNavigating;
        
        
    }
    /*
    private async void CheckNetworkAndLoadMap()
    {
        bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (hasInternet)
        {
            // CÓ MẠNG: Hiện WebView Goong
            GoongMapView.IsVisible = true;
            mapControl.IsVisible = false;
            OfflineWarning.IsVisible = false;

            // Chỉ load lại nếu Source đang trống hoặc sai
            if (GoongMapView.Source == null)
                GoongMapView.Source = "http://192.168.1.40:5500/goong_map.html";
        }
        else
        {
            // MẤT MẠNG: Hiện Mapsui Offline
            GoongMapView.IsVisible = false;
            mapControl.IsVisible = true;
            OfflineWarning.IsVisible = true;

            LoadOfflineMap();
        }
    }

   /* private async void LoadOfflineMap()
    {
        try
        {
            string fileName = "Unnamed atlas.mbtiles";
            string path = Path.Combine(FileSystem.AppDataDirectory, fileName);

            if (!File.Exists(path))
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var dest = File.Create(path);
                await stream.CopyToAsync(dest);
            }

            mapControl.Map = new Mapsui.Map();

            // ✅ DÙNG sqlite-net (đúng với BruTile)
            var mbtilesSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var tileLayer = new TileLayer(mbtilesSource);

            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(tileLayer);

            // DEBUG
            System.Diagnostics.Debug.WriteLine($"Extent: {tileLayer.Extent}");

            // ✅ convert đúng hệ tọa độ
            var mercator = Mapsui.Projections.SphericalMercator.FromLonLat(106.7, 10.7);
            mapControl.Map.Navigator.CenterOn(new Mapsui.MPoint(mercator.x, mercator.y));
            mapControl.Map.Navigator.ZoomTo(1000);

            mapControl.Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("MAP ERROR: " + ex.ToString());
            await DisplayAlert("Lỗi", ex.ToString(), "OK");
        }
    } */
    /*
    private void UpdateMapState(object sender, ConnectivityChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            CheckNetworkAndLoadMap();
        });
    }
    */
    // ================= WEBVIEW =================
    private void ConfigureWebView()
    {
        GoongMapView.HandlerChanged += (s, e) =>
        {
#if ANDROID
            if (GoongMapView.Handler?.PlatformView is Android.Webkit.WebView webView)
            {
                webView.Settings.JavaScriptEnabled = true;
                webView.Settings.DomStorageEnabled = true;
                webView.Settings.SetGeolocationEnabled(true);

                webView.Touch += (sender, touchEventArgs) =>
{
    // Yêu cầu tất cả cấp cha (bao gồm cả ScrollView) không được chặn sự kiện
    // khi ngón tay đang thao tác bên trong WebView
    webView.Parent?.RequestDisallowInterceptTouchEvent(true);

    if (touchEventArgs.Event.Action == Android.Views.MotionEventActions.Up || 
        touchEventArgs.Event.Action == Android.Views.MotionEventActions.Cancel)
    {
        // Chỉ trả lại quyền cuộn cho ScrollView khi người dùng nhấc tay lên
        webView.Parent?.RequestDisallowInterceptTouchEvent(false);
    }

    touchEventArgs.Handled = false; // Rất quan trọng để JS bên trong vẫn nhận được sự kiện
};

            }
#endif
        };
    }

    // ================= MAP SIZE =================
    private async void OnToggleMapSizeClicked(object sender, EventArgs e)
    {
        bool isFull = MapFrame.HeightRequest > 300;

        if (!isFull)
        {
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            MapFrame.HeightRequest = screenHeight - 150;

            // Cuộn trang lên đầu
            await ParentScroll.ScrollToAsync(MapFrame, ScrollToPosition.Start, true);
        }
        else
        {
            MapFrame.HeightRequest = 250;
        }

        // Đợi 300ms để hiệu ứng cuộn của ScrollView kết thúc hoàn toàn
        await Task.Delay(300);

        // ÉP WebView lấy lại quyền điều khiển
        GoongMapView.Focus();

        // Gọi hàm resize trong JS để bản đồ nhận diện lại vùng chạm mới
        await GoongMapView.EvaluateJavaScriptAsync("resizeMap();loadPOIsFromDatabase();");
    }



    // ================= GPS =================
    private async Task StartTrackingLocation()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(
                        GeolocationAccuracy.High,
                        TimeSpan.FromSeconds(10)
                    );

                    var location = await Geolocation.Default.GetLocationAsync(request, _cts.Token);

                    if (location != null)
                    {
                        string lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
                        string lng = location.Longitude.ToString(CultureInfo.InvariantCulture);

                        string js = $"if(typeof updateLocationFromNative==='function') updateLocationFromNative({lat},{lng});";

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await GoongMapView.EvaluateJavaScriptAsync(js);
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GPS INNER ERROR: {ex}");
                }

                await Task.Delay(2000, _cts.Token);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GPS ERROR: {ex}");
        }
    }

    // ================= LIFECYCLE =================
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        GoongMapView.Navigated -= OnWebViewNavigated;
        GoongMapView.Navigated += OnWebViewNavigated;

        await LoadData();
    }

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        if (_isTrackingStarted) return;

        _isTrackingStarted = true;
        await StartTrackingLocation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _cts?.Cancel();
        _isTrackingStarted = false;
    }

    // ================= SQL =================
    private async Task LoadData()
    {
        try
        {
            // 1. Lấy toàn bộ danh sách POI từ SQL
            var data = await _sqlService.GetTourismSitesRawAsync();

            if (data != null && data.Count > 0)
            {
                // Hiển thị tên của điểm đầu tiên làm mặc định (nếu muốn)
                MySingleLabel.Text = data[0].TourismName;

                // 2. Chuyển danh sách POI thành chuỗi JSON để gửi vào JavaScript
                var jsonPoints = System.Text.Json.JsonSerializer.Serialize(data);

                // 3. Gọi hàm JS trong file HTML để vẽ Marker
                // (Bạn cần viết hàm loadMarkers(data) trong file goong_map.html)
                string js = $"if(typeof loadMarkers === 'function') loadMarkers({jsonPoints});";
                await GoongMapView.EvaluateJavaScriptAsync(js);
            }
        }
        catch (Exception ex)
        {
            MySingleLabel.Text = "Lỗi tải dữ liệu";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }


    // ================= SPEECH =================
    private async void OnTestSpeechClicked(object sender, EventArgs e)
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();

        var voice = locales.FirstOrDefault(l => l.Language == "vi")
                    ?? locales.FirstOrDefault();

        await TextToSpeech.Default.SpeakAsync("Test giọng nói", new SpeechOptions
        {
            Locale = voice,
            Pitch = 0.8f,
            Volume = 1
        });
    }

    // ================= BUTTON GPS =================
    private async void OnGetLocationClicked(object sender, EventArgs e)
    {
        try
        {
            // Dùng High thay vì Best để phản hồi nhanh hơn
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(3));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                // Quan trọng: Dùng CultureInfo để tránh lỗi dấu phẩy ở VN
                string lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
                string lng = location.Longitude.ToString(CultureInfo.InvariantCulture);

                // Truyền trực tiếp vào hàm updateLocation (đổi tên cho đồng bộ)
                string js = $"isFollowing=true; updateLocation({lat}, {lng});";
                await GoongMapView.EvaluateJavaScriptAsync(js);
            }
        }
        catch (Exception ex) { /* Log error */ }
    }
    int currentLangId = 1;

    private async void OnGoongMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        // Kiểm tra URL bắt đầu bằng tiền tố chúng ta tự định nghĩa trong JS
        if (e.Url.StartsWith("poi-click://"))
        {
            e.Cancel = true; // Chặn không cho WebView chuyển trang thật

            try
            {
                // Giải mã chuỗi JSON từ URL
                string jsonPart = e.Url.Substring("poi-click://".Length);
                string decodedJson = System.Net.WebUtility.UrlDecode(jsonPart);

                // Chuyển JSON thành Object TourismSite
                var selectedSite = JsonSerializer.Deserialize<TourismSite>(decodedJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (selectedSite != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Đổ dữ liệu vào giao diện thông qua BindingContext
                        this.BindingContext = selectedSite;

                        // Cập nhật thủ công label địa chỉ nếu cần
                        addrLabel.Text = selectedSite.Address;
                        lblQuickInfo.Text = selectedSite.QuickInfo;
                    });

                    // --- PHẦN BỔ SUNG: Lấy danh sách Tour ---
                    var tours = await _sqlService.GetToursBySiteIdAsync(selectedSite.SiteID);
                    System.Diagnostics.Debug.WriteLine($"SỐ LƯỢNG TOUR TÌM THẤY: {tours?.Count ?? 0}");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // ToursListView là tên x:Name của CollectionView/BindableLayout trong XAML
                        ToursListView.ItemsSource = tours;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi nhận POI: " + ex.Message);
            }
        }
    }
    
    bool isMenuOpen = false;

    private void OnToggleLanguageMenu(object sender, EventArgs e)
    {
        isMenuOpen = !isMenuOpen;
        LangMenu.IsVisible = isMenuOpen;
    }

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

    private async void OnCheckVoicesClicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Lấy danh sách giọng đọc từ hệ thống
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // 2. Lọc ra các giọng Tiếng Việt
            var viVoices = locales.Where(l => l.Language.StartsWith("vi", StringComparison.OrdinalIgnoreCase)).ToList();

            if (viVoices.Count > 0)
            {
                string report = $"Tìm thấy {viVoices.Count} giọng Tiếng Việt:\n\n";

                foreach (var voice in viVoices)
                {
                    report += $"- {voice.Name}\n";

                    // Đọc thử tên từng giọng để bạn nghe xem là Nam hay Nữ
                    var options = new SpeechOptions { Locale = voice, Pitch = 1.0f };
                    await TextToSpeech.Default.SpeakAsync($"Đây là {voice.Name}", options);

                    // Nghỉ một chút giữa các giọng để dễ nghe
                    await Task.Delay(500);
                }

                await DisplayAlert("Kết quả", report, "Đã nghe xong");
            }
            else
            {
                await DisplayAlert("Thông báo", "Máy không có gói Tiếng Việt offline. App sẽ dùng giọng mặc định.", "OK");
                await TextToSpeech.Default.SpeakAsync("Đây là giọng đọc mặc định của hệ thống.");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", ex.Message, "OK");
        }
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
        lblVoiceOver.Text = AppResources.VoiceOver;
        lblSelectTour.Text = AppResources.SelectTour;
        lblCreateTour.Text = AppResources.CreateTour;
    }




}
