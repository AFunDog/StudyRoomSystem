using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using ShadUI;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.AvaloniaApp.Services;
using StudyRoomSystem.Core.Structs.Api;
using Zeng.CoreLibrary.Toolkit.Logging;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private ILogger Logger { get; }
    private IUserProvider UserProvider { get; }
    private IAuthApiService AuthApiService { get; }
    private INavigateService NavigateService { get; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    public LoginViewModel() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    public LoginViewModel(
        ILogger logger,
        IUserProvider userProvider,
        IAuthApiService authApiService,
        INavigateService navigateService)
    {
        Logger = logger.ForContext<LoginViewModel>();
        UserProvider = userProvider;
        AuthApiService = authApiService;
        NavigateService = navigateService;
    }

    [RelayCommand]
    private async Task Login()
    {
        try
        {
            await AuthApiService.Login(
                new()
                {
                    UserName = UserName,
                    Password = Password
                },
                async (res, loginResponse) =>
                {
                    UserProvider.User = loginResponse.User;
                    Logger.Trace().Information("登录成功 {@Res}", loginResponse);
                    Service.ToastManager.CreateToast("登录成功").ShowSuccess();
                    NavigateService.Navigate("/home");
                },
                async (res, error) =>
                {
                    Logger.Trace().Error("登录失败 {@Error}", error);
                    Service.ToastManager.CreateToast("登录失败").WithContent(error.Title).ShowError();
                }
            );
        }
        catch (Exception e)
        {
            Logger.Trace().Error(e, "登录");
            Service.ToastManager.CreateToast("登录失败").ShowError();
        }
    }

    [RelayCommand]
    private void ToRegisterView()
    {
        NavigateService.Navigate("/register");
    }
}