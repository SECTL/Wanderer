using Avalonia.Controls;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Services.Config;

namespace Wanderer.Views.MainPages;

[MainPageInfo("设置", "settings", "\uEF27")]
public partial class SettingsPage : UserControl
{
    public MainConfigHandler MainConfigHandler { get; } = IAppHost.GetService<MainConfigHandler>();
    
    public SettingsPage()
    {
        DataContext = this;
        InitializeComponent();
    }
}