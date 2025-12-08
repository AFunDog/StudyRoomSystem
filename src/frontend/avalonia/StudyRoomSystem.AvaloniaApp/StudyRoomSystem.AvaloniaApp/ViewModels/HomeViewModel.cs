using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShadUI;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.AvaloniaApp.Controls;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Entity;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public sealed partial class HomeViewModel : ViewModelBase
{
    private IRoomApiService RoomApiService { get; }
    private IBookingApiService BookingApiService { get; }

    [ObservableProperty]
    public partial bool IsUserSheetOpen { get; set; }

    [ObservableProperty]
    public partial Room? SelectedRoom { get; set; }

    public AvaloniaList<Seat> SelectedRoomSeats { get; set; } = [];
    [ObservableProperty]
    public partial int SelectedRoomWidth { get; set; }
    [ObservableProperty]
    public partial int SelectedRoomHeight { get; set; }

    // public AvaloniaDictionary<Guid, Booking> MyBookings { get; set; } = new()
    // {
    //     [Guid.NewGuid()] = new Booking()
    //     {
    //         Id = Guid.NewGuid(),
    //         UserId = Guid.NewGuid(),
    //         CreateTime = DateTime.UtcNow,
    //         StartTime = DateTime.UtcNow,
    //         EndTime = DateTime.UtcNow.AddHours(1),
    //         SeatId = Guid.NewGuid(), State = BookingStateEnum.Booking
    //     }
    // };

    private SourceCache<Room, Guid> RoomCache { get; } = new(x => x.Id);
    public IReadOnlyCollection<Room> Rooms { get; }

    private SourceCache<Booking, Guid> MyBookingCache { get; } = new(x => x.Id);
    public IReadOnlyCollection<Booking> MyBookings { get; }


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    public HomeViewModel()
    {
        #region 示例数据

        MyBookingCache.Connect().Bind(out var myBookings).Subscribe();
        MyBookingCache.AddOrUpdate(
            new Booking()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreateTime = DateTime.UtcNow.ToLocalTime(),
                StartTime = DateTime.UtcNow.ToLocalTime(),
                EndTime = DateTime.UtcNow.AddHours(1).ToLocalTime(),
                SeatId = Guid.NewGuid(), State = BookingStateEnum.Booking,
                Seat = new()
                {
                    Id = Guid.NewGuid(),
                    RoomId = Guid.NewGuid(),
                    Col = 1,
                    Row = 1,
                    Room = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room1",
                        CloseTime = default,
                        OpenTime = default,
                        Cols = 16,
                        Rows = 16
                    }
                }
            }
        );
        MyBookingCache.AddOrUpdate(
            new Booking()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreateTime = DateTime.UtcNow.ToLocalTime(),
                StartTime = DateTime.UtcNow.ToLocalTime(),
                EndTime = DateTime.UtcNow.AddHours(1).ToLocalTime(),
                SeatId = Guid.NewGuid(), State = BookingStateEnum.Booking,
                Seat = new()
                {
                    Id = Guid.NewGuid(),
                    RoomId = Guid.NewGuid(),
                    Col = 1,
                    Row = 1,
                    Room = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room2",
                        CloseTime = default,
                        OpenTime = default,
                        Cols = 16,
                        Rows = 16
                    }
                }
            }
        );

        MyBookings = myBookings;

        RoomCache.Connect().Bind(out var rooms).Subscribe();
        Rooms = rooms;

        #endregion
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

    public HomeViewModel(IRoomApiService roomApiService, IBookingApiService bookingApiService)
    {
        RoomApiService = roomApiService;
        BookingApiService = bookingApiService;

        MyBookingCache.Connect().Bind(out var myBookings).Subscribe();
        MyBookings = myBookings;

        RoomCache.Connect().Bind(out var rooms).Subscribe();
        Rooms = rooms;

        InitDataCommand.Execute(null);
    }

    [RelayCommand]
    private void InitData()
    {
        Log.Logger.Trace().Information("初始化数据");
        RoomApiService.GetAll(async (s, e) => { RoomCache.AddOrUpdate(e); });
        BookingApiService.GetMy(async (s, e) => { MyBookingCache.AddOrUpdate(e); });
    }

    [RelayCommand]
    private void ToggleUserSheet()
    {
        IsUserSheetOpen = !IsUserSheetOpen;
        Log.Logger.Trace().Information("ToggleUserSheet {IsOpen}", IsUserSheetOpen);
    }

    [RelayCommand]
    private void SelectRoom(Room room)
    {
        SelectedRoom = room;
        SelectedRoomSeats.Clear();
        SelectedRoomSeats.AddRange(room.Seats);
        SelectedRoomWidth = room.Rows * 32;
        SelectedRoomHeight = room.Cols * 32;
    }

    [RelayCommand]
    private async Task OpenSeatDialog(Seat seat)
    {
        var viewModel = Service.ServiceProvider.GetRequiredService<SeatInfoContentViewModel>();
        viewModel.Seat = seat;
        var res = await Service.DialogManager.Show<SeatInfoContent, Booking>(
            new SeatInfoContent()
            {
                DataContext = viewModel
            }
        );
        Log.Logger.Trace().Information("Show Dialog {@Res}", res);
        // var res = await DialogHost.Show(
        //     new SeatInfoContent()
        //     {
        //         DataContext = Service.ServiceProvider.GetRequiredService<SeatInfoContentViewModel>()
        //     }
        // );
        // Log.Logger.Trace().Information("OpenSeatDialog {Res}", res);

        // var viewModel = Service.ServiceProvider.GetRequiredService<SeatInfoContentViewModel>();
        // viewModel.Seat = seat;
        // Service.DialogManager.CreateDialog(viewModel)
        //     .Dismissible()
        //     .WithMaxWidth(512)
        //     .WithCancelCallback(() => { })
        //     .WithSuccessCallback(() => { })
        //     .Show();
        // Log.Logger.Trace().Information("OpenSeatDialog {Seat}", seat);
    }
}