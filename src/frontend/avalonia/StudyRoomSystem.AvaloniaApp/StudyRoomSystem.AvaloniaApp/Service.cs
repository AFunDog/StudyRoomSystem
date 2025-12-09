using System;
using System.Net;
using System.Net.Http;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using ShadUI;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.AvaloniaApp.Controls;
using StudyRoomSystem.AvaloniaApp.Pages;
using StudyRoomSystem.AvaloniaApp.Services;
using StudyRoomSystem.AvaloniaApp.Services.Api;
using StudyRoomSystem.AvaloniaApp.ViewModels;
using StudyRoomSystem.AvaloniaApp.Views;
using Zeng.CoreLibrary.Toolkit.Extensions;
using Zeng.CoreLibrary.Toolkit.Logging;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;
using DialogManager = StudyRoomSystem.AvaloniaApp.Services.DialogManager;
using Window = Avalonia.Controls.Window;

namespace StudyRoomSystem.AvaloniaApp;

public static class Service
{
    public static IServiceProvider ServiceProvider => App.ServiceProvider;
    public static ToastManager ToastManager => App.ServiceProvider.GetRequiredService<ToastManager>();
    public static DialogManager DialogManager { get; } = new();

    public static IServiceProvider BuildServices()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromTraceInfo()
            .Enrich.ShortSourceContext()
            // .Enrich.With<TraceInfoEnricher>()
#if DEBUG
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss fff} {Level:u3}] {TraceInfo}{Message:lj}{NewLine}{Exception}"
            )
#endif
            .WriteTo.File(
                new CompactJsonFormatter(),
                "logs/log-.json",
                restrictedToMinimumLevel: LogEventLevel.Verbose,
                rollingInterval: RollingInterval.Day
            )
            .CreateLogger();
        var services = new ServiceCollection()
            // 日志
            .AddSingleton<ILogger>(s => Log.Logger)
            .AddTransient<HttpLogHandler>()
            // HTTP
            .AddHttpClient(
                "API",
                client =>
                {
                    client.BaseAddress = new Uri("https://localhost:7175");
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                }
            )
            .AddHttpMessageHandler(p => p.GetRequiredService<HttpLogHandler>())
            // .AddHttpMessageHandler(p =>
            //     {
            //         // var tokenHandler = p.GetRequiredService<TokenHandler>();
            //         // return tokenHandler;
            //         return new HttpClientHandler()
            //         {
            //             
            //         };
            //     }
            // )
            .Services
            // 通知管理
            .AddSingleton<ToastManager>()
            // 用户信息
            .AddSingleton<IUserProvider, UserProvider>()
            // API
            .AddSingleton<IAuthApiService, HttpAuthApiService>()
            .AddSingleton<IUserApiService, HttpUserApiService>()
            .AddSingleton<IBookingApiService, HttpBookingApiService>()
            .AddSingleton<IRoomApiService, HttpRoomApiService>()
            // token处理
            // .AddTransient<TokenHandler>()
            // JWT
            // .AddSingleton<ITokenProvider, JsonWebTokenProvider>()
            // 导航
            .UseNavigateService()
            // 页面
            .AddKeyedSingleton<Window>(
                "MainWindow",
                (s, _) => new MainWindow() { DataContext = s.GetRequiredService<MainWindowViewModel>() }
            )
            .AddKeyedSingleton<UserControl>(
                "MainView",
                (s, _) => new MainView() { DataContext = s.GetRequiredService<MainViewModel>() }
            )
            .AddKeyedSingleton<UserControl>(
                "LoginView",
                (s, _) => new LoginView()
                {
                    DataContext = s.GetRequiredService<LoginViewModel>()
                }
            )
            .AddKeyedSingleton<UserControl>(
                "RegisterView",
                (s, _) => new RegisterView()
                {
                    DataContext = s.GetRequiredService<RegisterViewModel>()
                }
            )
            .AddKeyedSingleton<UserControl>(
                "HomeView",
                (s, _) => new HomeView()
                {
                    DataContext = s.GetRequiredService<HomeViewModel>()
                }
            )
            // 视图模型
            .AddTransient<MainWindowViewModel>()
            .AddTransient<MainViewModel>()
            .AddTransient<LoginViewModel>()
            .AddTransient<RegisterViewModel>()
            .AddTransient<HomeViewModel>()
            .AddTransient<SeatInfoContentViewModel>();


        var provider = services.BuildServiceProvider();

        RegisterRoutes(provider);
        // RegisterDialogs(provider);


        Log.Logger.Trace().Information("服务配置完毕");
        return provider;
    }

    private static void RegisterRoutes(IServiceProvider provider)
    {
        // 注册导航
        var navigator = provider.GetRequiredService<INavigateService>();
        navigator
            .RegisterViewRoute("/login", () => provider.GetRequiredKeyedService<UserControl>("LoginView"))
            .RegisterViewRoute("/register", () => provider.GetRequiredKeyedService<UserControl>("RegisterView"))
            .RegisterViewRoute("/home", () => provider.GetRequiredKeyedService<UserControl>("HomeView"));
    }
    
    private static void InitHttpApi(IServiceProvider provider)
    {
        var httpApi = provider.GetRequiredService<IHttpClientFactory>().CreateClient("API");
    }

    // private static void RegisterDialogs(IServiceProvider provider)
    // {
    //     var dialogManager = provider.GetRequiredService<DialogManager>();
    //     dialogManager.Register<SeatInfoContent, SeatInfoContentViewModel>();
    // }
}