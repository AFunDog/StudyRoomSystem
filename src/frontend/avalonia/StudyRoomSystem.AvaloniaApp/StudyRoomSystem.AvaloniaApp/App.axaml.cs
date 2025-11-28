using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using StudyRoomSystem.AvaloniaApp.Pages;
using StudyRoomSystem.AvaloniaApp.ViewModels;
using StudyRoomSystem.AvaloniaApp.Views;
using Zeng.CoreLibrary.Toolkit.Extensions;
using Zeng.CoreLibrary.Toolkit.Logging;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;

namespace StudyRoomSystem.AvaloniaApp;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; } = BuildServices();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            // desktop.MainWindow = new MainWindow
            // {
            //     DataContext = new MainWindowViewModel()
            // };
            desktop.MainWindow = ServiceProvider.GetRequiredKeyedService<Window>("MainWindow");
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // singleViewPlatform.MainView = new MainView
            // {
            //     DataContext = new MainWindowViewModel()
            // };
            singleViewPlatform.MainView = ServiceProvider.GetRequiredKeyedService<UserControl>("MainView");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove
            = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private static IServiceProvider BuildServices()
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
// #if RELEASE
//         .Console(
//                 outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext.Short}{Message:lj}{NewLine}{Exception}"
//             )
// #endif
            // .WriteTo.Logger(DebugConsoleLogger.Instance, restrictedToMinimumLevel: LogEventLevel.Verbose)
            // .WriteTo.File(
            //     "logs/log-.txt",
            //     outputTemplate:
            //     "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
            //     restrictedToMinimumLevel: LogEventLevel.Verbose,
            //     rollingInterval: RollingInterval.Day
            // )
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
            .AddHttpClient("API",client =>
                {
                    client.BaseAddress = new Uri("https://localhost:5106");
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                }
            )
            .Services
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
            // 视图模型
            .AddTransient<MainWindowViewModel>()
            .AddTransient<MainViewModel>()
            .AddTransient<LoginViewModel>();


        var provider = services.BuildServiceProvider();

        var navigator = provider.GetRequiredService<INavigateService>();
        navigator.RegisterViewRoute("/login", () => provider.GetRequiredKeyedService<UserControl>("LoginView"));


        Log.Logger.Trace().Information("服务配置完毕");
        return provider;
    }
}