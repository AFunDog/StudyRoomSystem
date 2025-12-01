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
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Server.Structs;
using Zeng.CoreLibrary.Toolkit.Logging;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private ILogger Logger { get; }
    private IHttpClientFactory HttpClientFactory { get; }
    private INavigateService NavigateService { get; }
    private ITokenProvider TokenProvider { get; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    public LoginViewModel() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    public LoginViewModel(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        INavigateService navigateService,
        ITokenProvider tokenProvider)
    {
        Logger = logger.ForContext<LoginViewModel>();
        HttpClientFactory = httpClientFactory;
        NavigateService = navigateService;
        TokenProvider = tokenProvider;
    }

    [RelayCommand]
    private async Task Login()
    {
        try
        {
            var client = HttpClientFactory.CreateClient("API");
            var res = await client.PostAsync(
                "/api/v1/auth/login",
                new StringContent(
                    JsonSerializer.Serialize(
                        new LoginRequest()
                        {
                            UserName = UserName,
                            Password = Password
                        },
                        AppJsonSerializerContext.Default.LoginRequest
                    ),
                    Encoding.UTF8,
                    "application/json"
                )
            );
            if (res.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponseOk>(
                    await res.Content.ReadAsStringAsync(),
                    AppJsonSerializerContext.Default.LoginResponseOk
                );
                ArgumentNullException.ThrowIfNull(loginResponse);
                TokenProvider.Token = loginResponse.Token;
                Logger.Trace().Information("登录成功 {@Res}", loginResponse);
                Service.ToastManager.CreateToast("登录成功").ShowSuccess();
            }
            else
            {
                var error = JsonSerializer.Deserialize<ResponseError>(
                    await res.Content.ReadAsStringAsync(),
                    AppJsonSerializerContext.Default.ResponseError
                );
                ArgumentNullException.ThrowIfNull(error);
                Logger.Trace().Error("登录失败 {@Error}", error);
                Service.ToastManager.CreateToast("登录失败").WithContent(error.Message).ShowError();
            }
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