using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Zeng.CoreLibrary.Toolkit.Logging;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ILogger Logger { get; }
    private INavigateService NavigateService { get; } 
    
    [ObservableProperty]
    public partial object? Content { get; set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    public MainViewModel() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

    public MainViewModel(ILogger logger,INavigateService navigateService)
    {
        Logger = logger.ForContext<MainViewModel>();
        NavigateService = navigateService;

        NavigateService.OnNavigated += (s, e) =>
        {
            Content = e;
        };
        
        NavigateService.Navigate("/login");
    }

    [RelayCommand]
    private void ViewLoaded()
    {
        Logger.Trace().Information("MainView 加载完毕");
        

    }
}
