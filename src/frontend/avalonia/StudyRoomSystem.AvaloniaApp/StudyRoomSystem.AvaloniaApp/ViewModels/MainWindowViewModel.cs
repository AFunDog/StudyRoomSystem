using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Zeng.CoreLibrary.Toolkit.Services.Navigate;

namespace StudyRoomSystem.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial object? Content { get; set; }
    
    public MainWindowViewModel() { }

    public MainWindowViewModel([FromKeyedServices("MainView")] UserControl mainView)
    {
        Content = mainView;
    }
}