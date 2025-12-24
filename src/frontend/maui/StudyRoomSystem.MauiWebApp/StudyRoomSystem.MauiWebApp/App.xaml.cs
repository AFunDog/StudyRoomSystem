namespace StudyRoomSystem.MauiWebApp;

public partial class App : Application
{
    public App()
    {
        /*
         * 在 Windows 上，使用已安装到 Program Files 目录的基于 WebView2 的控件的应用可能无法正确呈现内容。
         * 之所以发生这种情况，是因为 WebView2 尝试将其缓存和用户数据文件写入应用的安装目录，
         * 该目录具有受限的 Program Files写入权限。
         * 若要解决此问题，请在 WEBVIEW2_USER_DATA_FOLDER 初始化任何 WebView 控件之前设置环境变量：
         */
#if WINDOWS
var userDataFolder = Path.Combine(FileSystem.AppDataDirectory, "WebView2");
Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", userDataFolder);
#endif

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}