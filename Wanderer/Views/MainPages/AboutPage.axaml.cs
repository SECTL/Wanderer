using System;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Wanderer.Attributes;

namespace Wanderer.Views.MainPages;

[MainPageInfo("关于 Wanderer", "about", "\uE9E4")]
public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        DataContext = this;
        InitializeComponent();
    }

    [RelayCommand]
    private void OpenLink(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start(new ProcessStartInfo("xdg-open", url)
            {
                UseShellExecute = false
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", url)
            {
                UseShellExecute = false
            });
        }
    }
}