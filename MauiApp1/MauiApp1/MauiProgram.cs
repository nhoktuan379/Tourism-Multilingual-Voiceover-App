using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using ZXing.Net.Maui.Controls;
using Mapsui.UI.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

#if ANDROID
using Android.Webkit;
#endif

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SQLitePCL.Batteries.Init();
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseBarcodeReader()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if ANDROID
            builder.ConfigureMauiHandlers(handlers =>
            {
                Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("EnableGeoLocation", (handler, view) =>
                {
                    // Bổ sung các cấu hình cho GPS
                    handler.PlatformView.Settings.JavaScriptEnabled = true;
                    handler.PlatformView.Settings.SetGeolocationEnabled(true);
                    handler.PlatformView.Settings.SetGeolocationDatabasePath(handler.PlatformView.Context.FilesDir.Path);
                    // 2. Cho phép chạy HTTP (Không an toàn)
                    handler.PlatformView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
        
        
                    // QUAN TRỌNG NHẤT: Cấp quyền thông qua WebChromeClient
                    handler.PlatformView.SetWebChromeClient(new MyWebChromeClient());
                });
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

#if ANDROID
    // Class bổ sung để WebView đồng ý yêu cầu GPS từ JavaScript
    public class MyWebChromeClient : WebChromeClient
    {
        public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
        {
            callback.Invoke(origin, true, false);
        }
    }
#endif
}
