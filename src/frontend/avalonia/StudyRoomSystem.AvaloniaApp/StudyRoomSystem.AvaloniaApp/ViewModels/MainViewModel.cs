using CommunityToolkit.Mvvm.ComponentModel;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}