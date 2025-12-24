namespace StudyRoomSystem.MauiWebApp;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();


        Task.Run(LoadWeb);
    }

    private async Task LoadWeb()
    {
        var html = await FileSystem.OpenAppPackageFileAsync("wwwroot/index.html");
        using var reader = new StreamReader(html);
        
        WebView.Source = new HtmlWebViewSource
        {
            Html = await reader.ReadToEndAsync(),
            BaseUrl = "/"
        };
    }
}