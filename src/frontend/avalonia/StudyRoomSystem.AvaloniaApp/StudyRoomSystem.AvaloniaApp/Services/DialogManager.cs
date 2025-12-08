using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using StudyRoomSystem.AvaloniaApp.Controls;

namespace StudyRoomSystem.AvaloniaApp.Services;

public sealed partial class DialogManager
{
    public DialogHost? DialogHost { get; set; }


    [RelayCommand]
    public async Task<object?> Show(object? content = null)
    {
        if (DialogHost is null)
            return null;
        DialogHost.DialogResult = null;
        DialogHost.Content = content;
        DialogHost.IsOpen = true;

        await DialogHost.GetObservable(DialogHost.IsOpenProperty).Any(x => x is false).ToTask();

        return DialogHost.DialogResult;
    }

    public async Task<TOut?> Show<TIn, TOut>(TIn? content = default)
    {
        return (TOut?)await Show(content);
    }

    [RelayCommand]
    public void Close(object? result = null)
    {
        if (DialogHost is null)
            return;
        DialogHost.DialogResult = result;
        DialogHost.IsOpen = false;
    }
}