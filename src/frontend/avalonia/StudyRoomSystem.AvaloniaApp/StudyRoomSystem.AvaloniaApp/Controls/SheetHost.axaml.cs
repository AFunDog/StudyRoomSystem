using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace StudyRoomSystem.AvaloniaApp.Controls;

public class SheetHost : TemplatedControl
{
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<SheetHost, bool>(nameof(IsOpen));
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty
        = ContentControl.ContentProperty.AddOwner<SheetHost>();
    
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty
        = ContentControl.ContentTemplateProperty.AddOwner<SheetHost>();
    
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

}