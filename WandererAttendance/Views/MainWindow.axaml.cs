using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using FluentAvalonia.UI.Windowing;
using WandererAttendance.Abstraction;
using WandererAttendance.Controls;
using WandererAttendance.Enums;
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
        IAppHost.GetService<MainConfigHandler>().Data.PropertyChanged += (o, args) =>
        {
            ReloadConfig();
        };
        ReloadConfig();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        IAppHost.GetService<MainConfigHandler>().Save();
        IAppHost.GetService<ProfileConfigHandler>().Save();
    }

    private void ReloadConfig()
    {
        var isEffectSupported = OperatingSystem.IsWindows() 
                                && AvaloniaUnsafeAccessorHelpers.GetActiveWin32CompositionMode() == AvaloniaUnsafeAccessorHelpers.Win32CompositionMode.WinUIComposition;
        
        if (!isEffectSupported) return;

        var config = IAppHost.GetService<MainConfigHandler>().Data;
        var isMicaSupported = Environment.OSVersion.Version >= new Version(10, 0, 22000, 0);
        var isAcrylicSupported = Environment.OSVersion.Version >= new Version(10, 0, 18362, 0);
        
        if (isMicaSupported && config.BackgroundEffect == BackgroundEffect.Mica)
        {
            TransparencyLevelHint = [WindowTransparencyLevel.Mica];
            Background = Brushes.Transparent;
        }
        else if (isAcrylicSupported && config.BackgroundEffect == BackgroundEffect.Acrylic)
        {
            TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
            Background = Brushes.Transparent;
        }
        else
        {
            TransparencyLevelHint = [];
            Background = new SolidColorBrush(GetBackgroundColor());
        }
    }

    private Color GetBackgroundColor()
    {
        if (this.TryFindResource("ApplicationPageBackgroundThemeBrush", ActualThemeVariant, out var pageBackgroundRes)
            && pageBackgroundRes is ISolidColorBrush pageBackgroundBrush)
        {
            return pageBackgroundBrush.Color;
        }
        
        if (this.TryFindResource("SolidBackgroundFillColorBase", ActualThemeVariant, out var res)
            && res is Color color)
        {
            return color;
        }
        
        var appTheme = Application.Current?.RequestedThemeVariant ?? ThemeVariant.Default;
        var platformThemeVariant = PlatformSettings?.GetColorValues().ThemeVariant ?? PlatformThemeVariant.Light;
        if (appTheme == ThemeVariant.Default)
        {
            return platformThemeVariant == PlatformThemeVariant.Dark
                ? Color.Parse("#000000") : Color.Parse("#FFFFFF");
        }
        
        return appTheme == ThemeVariant.Dark ? Color.Parse("#000000") : Color.Parse("#FFFFFF");
    }
}