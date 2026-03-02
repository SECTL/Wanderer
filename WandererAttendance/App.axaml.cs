using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using WandererAttendance.Abstraction;
using WandererAttendance.Extensions.Registry;
using WandererAttendance.Services;
using WandererAttendance.Services.Config;
using WandererAttendance.Services.Logging;
using WandererAttendance.ViewModels;
using WandererAttendance.ViewModels.MainPages;
using WandererAttendance.Views;
using WandererAttendance.Views.MainPages;

namespace WandererAttendance;

public partial class App : Application
{
    public static bool IsDesktop { get; private set; } = false;
    public static MainWindow? MainWindow { get; private set; } = null;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BuildHost();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            IsDesktop = true;
            MainWindow = new MainWindow
            {
                Content = IAppHost.GetService<MainView>()
            };
            desktop.MainWindow = MainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            IsDesktop = false;
            singleViewPlatform.MainView = IAppHost.GetService<MainView>();
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

    private void BuildHost()
    {
        IAppHost.Host = Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(services =>
            {
                // 日志
                services.AddLogging(builder =>
                {
                    builder.AddConsoleFormatter<ClassIslandConsoleFormatter, ConsoleFormatterOptions>();
                    builder.AddConsole(console => { console.FormatterName = "classisland"; });
#if DEBUG
                    builder.SetMinimumLevel(LogLevel.Trace);
#endif
                });
                
                // 配置
                services.AddSingleton<ConfigServiceBase, DesktopConfigService>();
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
            })
            .Build();

        var logger = IAppHost.GetService<ILogger<App>>();
        logger.LogInformation("WandererAttendance Copyright by lrs2187(2026) Licensed under GPL3.0");
        logger.LogInformation("Host built.");
        
        var lifetime = IAppHost.GetService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(Stop);
        
        var mainConfigHandler = IAppHost.GetService<MainConfigHandler>();
        
        logger.LogInformation("当前档案：{PROFILE_NAME}", mainConfigHandler.Data.ProfileName);
        ProfileService.ProfileName = mainConfigHandler.Data.ProfileName;
        IAppHost.GetService<ProfileConfigHandler>();

        _ = IAppHost.Host.StartAsync();
    }
    
    public static void Stop()
    {
        var logger = IAppHost.GetService<ILogger<App>>();
        logger.LogInformation("正在停止应用");

        var configHandler = IAppHost.GetService<MainConfigHandler>();
        configHandler.Save();
        
        var profileConfigHandler = IAppHost.GetService<ProfileConfigHandler>();
        profileConfigHandler.Save();
    }
}
