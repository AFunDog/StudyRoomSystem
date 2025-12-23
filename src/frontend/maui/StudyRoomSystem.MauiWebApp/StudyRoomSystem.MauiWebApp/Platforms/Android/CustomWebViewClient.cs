using Android.Net.Http;
using Android.Webkit;
using WebView = Android.Webkit.WebView;

namespace StudyRoomSystem.MauiWebApp;

public class CustomWebViewClient : WebViewClient
{
    public override void OnReceivedSslError(WebView? view, SslErrorHandler? handler, SslError? error)
    {
        handler?.Proceed();
    }

    // public override bool ShouldOverrideUrlLoading(WebView? view, IWebResourceRequest? request)
    // {
    //     if (request?.Url?.ToString() is { } url)
    //         view?.LoadUrl(url);
    //     return true;
    // }
}