#if ANDROID
using Android.Webkit;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using WebView = Microsoft.Maui.Controls.WebView;

namespace StudyRoomSystem.MauiWebApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }
            );

#if ANDROID
        // builder.ConfigureMauiHandlers(handlers =>
        // {
        //     handlers.AddHandler<WebView, CustomWebViewHandler>();
        // });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
#if ANDROID
    
    class CustomWebViewHandler : WebViewHandler
    {
        protected override Android.Webkit.WebView CreatePlatformView()
        {
            var webView = base.CreatePlatformView();
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.AllowFileAccess = true;
            webView.Settings.AllowContentAccess = true;
            webView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            webView.Settings.AllowFileAccessFromFileURLs = true;
            webView.Settings.AllowUniversalAccessFromFileURLs = true;
            return webView;
        }
    }
#endif
}