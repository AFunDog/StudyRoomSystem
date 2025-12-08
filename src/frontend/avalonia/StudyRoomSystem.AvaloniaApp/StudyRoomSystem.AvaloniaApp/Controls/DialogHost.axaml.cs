using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using StudyRoomSystem.AvaloniaApp.Services;

namespace StudyRoomSystem.AvaloniaApp.Controls;

[TemplatePart("PART_Overlay",typeof(Control))]
public class DialogHost : TemplatedControl
{
    
    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<DialogHost, object>(nameof(Content));
    
    [Content]
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<DialogHost, bool>(nameof(IsOpen));
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    public static readonly StyledProperty<DialogManager> DialogManagerProperty =
        AvaloniaProperty.Register<DialogHost, DialogManager>(nameof(DialogManager));
    
    public DialogManager DialogManager
    {
        get => GetValue(DialogManagerProperty);
        set => SetValue(DialogManagerProperty, value);
    }

    public static readonly StyledProperty<object?> DialogResultProperty
        = AvaloniaProperty.Register<DialogHost, object?>(nameof(DialogResult));
    
    public object? DialogResult
    {
        get => GetValue(DialogResultProperty);
        set => SetValue(DialogResultProperty, value);
    }

    static DialogHost()
    {
        DialogManagerProperty.Changed.AddClassHandler((DialogHost s,AvaloniaPropertyChangedEventArgs<DialogManager> e) =>
        {
            if (e.OldValue.HasValue)
            {
                e.OldValue.Value?.DialogHost = null;
            }
            if (e.NewValue.HasValue)
            {
                e.NewValue.Value?.DialogHost = s;
            }
        });
    }


}