using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using WandererAttendance.Abstraction;
using WandererAttendance.Controls;
using WandererAttendance.Helpers;
using WandererAttendance.Services.Config;

namespace WandererAttendance.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        SplashScreen = new EmptySplashScreen();
        InitializeComponent();

        TitleBar.Height = 48;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var isMicaSupported = OperatingSystem.IsWindows() 
                              && Environment.OSVersion.Version >= new Version(10, 0, 22000, 0)
                              && AvaloniaUnsafeAccessorHelpers.GetActiveWin32CompositionMode() == AvaloniaUnsafeAccessorHelpers.Win32CompositionMode.WinUIComposition;
        if (isMicaSupported)
        {
            TransparencyLevelHint = [WindowTransparencyLevel.Mica];
            Background = Brushes.Transparent;
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        IAppHost.GetService<ProfileConfigHandler>().Save();
    }
}