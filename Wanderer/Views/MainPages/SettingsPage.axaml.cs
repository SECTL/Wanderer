using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Services;
using Wanderer.Services.Config;
using Wanderer.Shared;

namespace Wanderer.Views.MainPages;

[MainPageInfo("设置", "settings", "\uEF27")]
public partial class SettingsPage : UserControl
{
    private const string DefaultStartupPageId = "home";

    public SettingsPage()
    {
        DataContext = this;
        InitializeComponent();

        InitializeStartupPageOptions();
    }

    public MainConfigHandler MainConfigHandler { get; } = IAppHost.GetService<MainConfigHandler>();

    public IReadOnlyList<StartupPageOption> StartupPageOptions { get; private set; } = [];

    public int StartupPageIndex
    {
        get
        {
            var index = StartupPageOptions
                        .ToList()
                        .FindIndex(option => option.Id == MainConfigHandler.Data.StartupPageId);
            return index < 0 ? 0 : index;
        }
        set
        {
            if (value >= 0 && value < StartupPageOptions.Count)
            {
                MainConfigHandler.Data.StartupPageId = StartupPageOptions[value].Id;
            }
        }
    }

    private void InitializeStartupPageOptions()
    {
        StartupPageOptions = MainPagesRegistryService.Items
                                                     .Where(info => !info.IsSeparator)
                                                     .Select(info => new StartupPageOption(info.Id, info.Name))
                                                     .ToList();
        NormalizeStartupPage();
    }

    private void NormalizeStartupPage()
    {
        if (StartupPageOptions.All(option => option.Id != MainConfigHandler.Data.StartupPageId))
        {
            MainConfigHandler.Data.StartupPageId = DefaultStartupPageId;
        }
    }

    private async void OpenDataFolderItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var path = Utils.GetDataDirectory();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // 目录可能还没被创建（例如全新安装、尚未写入任何配置）。
        Directory.CreateDirectory(path);

        try
        {
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            await CommonTaskDialogs.ShowDialog("无法打开数据目录",
                                               $"无法在文件管理器中打开 {path}。" + Environment.NewLine +
                                               Environment.NewLine + exception.Message);
        }
    }
}

/// <summary>
/// 「主界面」下拉框的候选项。名称取自导航注册表的页面名称，<see cref="ToString" /> 供下拉框直接显示。
/// </summary>
/// <param name="Id">页面 ID，对应 <c>MainPageInfo.Id</c>。</param>
/// <param name="Name">页面名称，对应 <c>MainPageInfo.Name</c>。</param>
public record StartupPageOption(string Id, string Name)
{
    public override string ToString() => Name;
}