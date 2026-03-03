using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using WandererAttendance.Controls;
using WandererAttendance.Helpers;

namespace WandererAttendance.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        SplashScreen = new EmptySplashScreen();
        InitializeComponent();

        Closed += Window_OnClosed;
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

    private void Window_OnClosed(object? sender, EventArgs? e)
    {
        App.Stop();
    }
}