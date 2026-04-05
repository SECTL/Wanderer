using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Attributes;
using Wanderer.Helpers.UI;

namespace Wanderer.Views.MainPages;

[MainPageInfo("调试", "debug", "\uE2C8")]
public partial class DebugPage : UserControl
{
    public DebugPage()
    {
        InitializeComponent();
    }

    private void ShowToastButton_OnClick(object? sender, RoutedEventArgs e)
    {
        this.ShowToast("测试测试~");
    }
}