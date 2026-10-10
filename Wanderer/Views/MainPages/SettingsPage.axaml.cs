using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Services;
using Wanderer.Services.Config;

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