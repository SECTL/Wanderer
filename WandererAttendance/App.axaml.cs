using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using WandererAttendance.Abstraction;
using WandererAttendance.Controls;
using WandererAttendance.Extensions.Registry;
using WandererAttendance.Services;
using WandererAttendance.Services.Config;
using WandererAttendance.Services.Logging;
using WandererAttendance.Shared;
using WandererAttendance.ViewModels;
using WandererAttendance.ViewModels.MainPages;
using WandererAttendance.Views;
using WandererAttendance.Views.MainPages;

namespace WandererAttendance;

public partial class App : Application
{
    public static IClassicDesktopStyleApplicationLifetime? Lifetime { get; private set; }
    public static bool IsDesktop { get; private set; } = false;
    public static MainWindow? MainWindow { get; private set; } = null;
    public static Window PhonyRootWindow = null!;

    public static bool IsStopping { get; set; } = false;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("zh-hans");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("zh-hans");

        BuildHost();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            Lifetime = desktop;
            IsDesktop = true;
            desktop.Startup += DesktopOnLifetimeStartup;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            IsDesktop = false;
            InitializeHost();
            
            singleViewPlatform.MainView = IAppHost.GetService<MainView>();
            if (OperatingSystem.IsBrowser())
            {
                singleViewPlatform.MainView.Classes.Add("browser");
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private async void DesktopOnLifetimeStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        CreatePhonyRootWindow();
        
        var mutex = new Mutex(true, "Global\\WandererAttendance.Lock", out var createNew);
        if (!createNew)
        {
            await ProcessInstanceExisted();
            Environment.Exit(0);
            return;
        }
        
        InitializeHost();
        ShowMainWindow();
    }
    
    private async Task ProcessInstanceExisted()
    {
        var dialog = new TaskDialog
        {
            Title = "易考勤 已在运行",
            Content = "易考勤 已经启动，请通过任务栏托盘图标进行设置等操作。",
            XamlRoot = GetRootWindow(),
            Buttons =
            [
                new TaskDialogButton("取消", false)
            ],
            Commands =
            [
                new TaskDialogCommand
                {
                    DialogResult = true,
                    ClosesOnInvoked = true,
                    Text = "重启当前实例",
                    Description = "结束正在运行的 易考勤 实例，然后再次启动本实例。",
                    IconSource = new FluentIconSource("\ue0bd"),
                }
            ]
        };
        var r = await dialog.ShowAsync();
        if (!Equals(r, true))
        {
            return;
        }
        try
        {
            var proc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Environment.ProcessPath))
                .Where(x=>x.Id != Environment.ProcessId);
            foreach (var i in proc)
            {
                i.Kill(true);
            }

            Restart();
        }
        catch (Exception e)
        {
            await CommonTaskDialogs
                .ShowDialog("重启失败", "无法重新启动应用，可能当前运行的实例正在以管理员身份运行。请使用任务管理器终止正在运行的实例，然后再试一次。"+Environment.NewLine+Environment.NewLine+$"{e.Message}");
        }
    }

    private static void BuildHost()
    {
        IAppHost.Host = Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(services =>
            {
                // 日志
                services.AddLogging(builder =>
                {
                    if (!OperatingSystem.IsBrowser())
                    {
                        builder.AddConsoleFormatter<ClassIslandConsoleFormatter, ConsoleFormatterOptions>();
                        builder.AddConsole(console => { console.FormatterName = "classisland"; });
                    }
#if DEBUG
                    builder.SetMinimumLevel(LogLevel.Trace);
#endif
                });
                
                // 配置
                if (OperatingSystem.IsBrowser())
                {
                    services.AddSingleton<ConfigServiceBase, BrowserConfigService>();
                }
                else
                {
                    services.AddSingleton<ConfigServiceBase, DesktopConfigService>();
                }
                services.AddSingleton<MainConfigHandler>();
                services.AddSingleton<ProfileConfigHandler>();
                
                // 服务
                services.AddSingleton<ProfileService>();
                
                // 主窗口
                services.AddSingleton<MainView>();
                services.AddTransient<MainViewModel>();
                
                // 界面 Views
                services.AddMainPage<HomePage>();
                services.AddMainPageSeparator();
                services.AddMainPage<AttendancePage>();
                services.AddMainPage<ProfilePage>();
                services.AddMainPage<HistoryPage>();
                services.AddMainPage<RankingPage>();

                services.AddMainPageFooter<AboutPage>();
                services.AddMainPageFooterSeparator();
                services.AddMainPageFooter<SettingsPage>();
#if DEBUG
                services.AddMainPageFooter<DebugPage>();
#endif
                
                // 界面 ViewModels
                services.AddTransient<HomePageViewModel>();
                services.AddTransient<AttendancePageViewModel>();
                services.AddTransient<ProfilePageViewModel>();
                services.AddTransient<HistoryPageViewModel>();
            })
            .Build();
    }

    private static void InitializeHost()
    {
        var logger = IAppHost.GetService<ILogger<App>>();
        logger.LogInformation("WandererAttendance Copyright by lrs2187(2026) Licensed under GPL3.0");
        logger.LogInformation("Host built.");
        
        var lifetime = IAppHost.GetService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(Stop);
        lifetime.ApplicationStopped.Register(() => logger.LogInformation("App Stopped."));
        
        var mainConfigHandler = IAppHost.GetService<MainConfigHandler>();
        
        logger.LogInformation("当前档案：{PROFILE_NAME}", mainConfigHandler.Data.ProfileName);
        ProfileService.ProfileName = mainConfigHandler.Data.ProfileName;
        IAppHost.GetService<ProfileConfigHandler>();

        _ = IAppHost.Host?.StartAsync();
    }
    
    private static void CreatePhonyRootWindow()
    {
        PhonyRootWindow = new Window
        {
            Width = 1,
            Height = 1,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowActivated = false,
            SystemDecorations = SystemDecorations.None,
            ShowInTaskbar = false,
            Background = Brushes.Transparent,
            TransparencyLevelHint = [ WindowTransparencyLevel.Transparent ],
            Title = "PhonyRootWindow"
        };
        
        PhonyRootWindow.Closing += (sender, args) =>
        {
            if (args.CloseReason is WindowCloseReason.ApplicationShutdown or WindowCloseReason.OSShutdown)
            {
                return;
            }
            args.Cancel = true;
        };
        
        PhonyRootWindow.Show();
    }
    
    public static Window GetRootWindow()
    {
        var w = Lifetime?.Windows
            .FirstOrDefault(x => x.GetType().Name != "TrayPopupRoot" && x is { IsActive: true, IsVisible: true });
        if (w != null) 
            return w;
        w = PhonyRootWindow;
        w.Activate();

        return w;
    }
    
    public static void Stop()
    {
        if (IsStopping) return;
        
        var logger = IAppHost.GetService<ILogger<App>>();
        IsStopping = true;
        
        Dispatcher.UIThread.Invoke(() =>
        {
            logger.LogInformation("正在停止应用");

            if (IsDesktop && MainWindow != null)
            {
                MainWindow.Close();
            }

            IAppHost.GetService<MainConfigHandler>().Save();
            IAppHost.GetService<ProfileConfigHandler>().Save();

            IAppHost.Host?.StopAsync(TimeSpan.FromSeconds(5));
            Lifetime?.Shutdown();
        });
    }

    public static void Restart()
    {
        var path = Environment.ProcessPath;
        if (path == null) return;
        
        var executablePath = path.Replace(".dll", GlobalConstants.PlatformExecutableExtension);
        var startInfo = new ProcessStartInfo(executablePath)
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    public static void ShowMainWindow()
    {
        if (!IsDesktop)
        {
            return;
        }
        
        if (MainWindow is not { IsLoaded: true })
        {
            MainWindow = new MainWindow
            {
                Content = IAppHost.GetService<MainView>()
            };
        }
        
        MainWindow.Show();
        MainWindow.Activate();
    }
    
    private void NativeMenuItemOpenMainWindow_OnClick(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void NativeMenuItemRestartApp_OnClick(object? sender, EventArgs e)
    {
        Stop();
        Restart();
    }

    private void NativeMenuItemExitApp_OnClick(object? sender, EventArgs e)
    {
        Stop();
    }
}
