using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.AvaloniaApp.Controls;

public partial class UserInfoSheet : UserControl
{
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<UserInfoSheet, bool>(nameof(IsOpen));
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    
    public static readonly StyledProperty<User?> UserProperty = AvaloniaProperty.Register<UserInfoSheet, User?>(nameof(User));
    
    public User? User
    {
        get => GetValue(UserProperty);
        set => SetValue(UserProperty, value);
    }
    
    
    public UserInfoSheet()
    {
        InitializeComponent();
    }
}