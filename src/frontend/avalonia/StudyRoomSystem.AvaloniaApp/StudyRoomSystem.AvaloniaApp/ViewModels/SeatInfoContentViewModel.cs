using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public sealed partial class SeatInfoContentViewModel : ViewModelBase
{
    private IBookingApiService BookingApiService { get; }

    [ObservableProperty]
    public partial Seat? Seat { get; set; }

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    [Required]
    public partial TimeSpan? CheckInTime { get; set; }

    [ObservableProperty]
    [Required]
    public partial TimeSpan? CheckOutTime { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    public SeatInfoContentViewModel() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

    public SeatInfoContentViewModel(IBookingApiService bookingApiService)
    {
        BookingApiService = bookingApiService;

        BookCommand.PropertyChanged += (s, e) => { CancelBookCommand.NotifyCanExecuteChanged(); };
    }
    

    [RelayCommand]
    private async Task Book()
    {
        ClearErrors();
        ValidateAllProperties();
        ErrorMessage = null;
        
        if (Seat is null || CheckInTime is null || CheckOutTime is null)
            return;
        try
        {
            await BookingApiService.Create(
                new CreateBookingRequest()
                {
                    SeatId = Seat.Id,
                    StartTime = SelectedDate.Add(CheckInTime.Value).ToUniversalTime(),
                    EndTime = SelectedDate.Add(CheckOutTime.Value).ToUniversalTime()
                },
                async (s, e) => { Service.DialogManager.Close(e); },
                async (s, e) => { ErrorMessage = $"预约失败 {e.Title}"; }
            );
        }
        catch (Exception e)
        {
            ErrorMessage = $"预约失败 {e.Message}";
            Log.Logger.Trace().Error(e, "尝试预约");
        }
    }

    public bool CanCancelBook => !BookCommand.IsRunning;

    [RelayCommand(CanExecute = nameof(CanCancelBook))]
    private void CancelBook()
    {
        Service.DialogManager.Close();
    }
}