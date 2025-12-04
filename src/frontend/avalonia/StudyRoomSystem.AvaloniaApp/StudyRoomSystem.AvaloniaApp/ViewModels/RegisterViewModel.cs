using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ShadUI;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using Zeng.CoreLibrary.Toolkit.Logging;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private ILogger Logger { get; }
    private INavigateService NavigateService { get; }
    private IUserApiService UserApiService { get; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    public RegisterViewModel() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

    [ObservableProperty]
    [Required]
    public partial string UserName { get; set; } = string.Empty;
    partial void OnUserNameChanged(string value) => ValidateProperty(value, nameof(UserName));

    [ObservableProperty]
    [Required]
    public partial string Password { get; set; } = string.Empty;
    partial void OnPasswordChanged(string value) => ValidateProperty(value, nameof(Password));

    [ObservableProperty]
    [Required]
    public partial string CampusId { get; set; } = string.Empty;
    partial void OnCampusIdChanged(string value) => ValidateProperty(value, nameof(CampusId));

    [ObservableProperty]
    [Required]
    [Phone]
    public partial string Phone { get; set; } = string.Empty;
    partial void OnPhoneChanged(string value) => ValidateProperty(value, nameof(Phone));

    // [ObservableProperty]
    // public partial string Phone { get; set; }

    public RegisterViewModel(ILogger logger, INavigateService navigateService, IUserApiService userApiService)
    {
        Logger = logger.ForContext<RegisterViewModel>();
        NavigateService = navigateService;
        UserApiService = userApiService;
    }

    [RelayCommand]
    private async Task Register()
    {
        ClearErrors();
        ValidateAllProperties();

        if (HasErrors)
            return;

        try
        {
            var res = UserApiService.Register(
                new RegisterRequest()
                {
                    UserName = UserName,
                    Password = Password,
                    Phone = Phone,
                    CampusId = CampusId
                },
                async (res, user) =>
                {
                    Logger.Trace().Information("注册成功 {@Res}", user);
                    Service.ToastManager.CreateToast("注册成功，请前往登录").ShowSuccess();
                    NavigateService.Navigate("/login");
                },
                async (res, error) =>
                {
                    Logger.Trace().Error("注册失败 {@Res}", error);
                    Service.ToastManager.CreateToast("注册失败").WithContent(error.Title).ShowError();
                }
            );
        }
        catch (Exception e)
        {
            Logger.Trace().Error(e, "注册");
            Service.ToastManager.CreateToast("注册失败").ShowError();
        }
    }

    [RelayCommand]
    private void BackToLoginView()
    {
        NavigateService.Back();
    }
}