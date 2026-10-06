using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Wanderer.Attributes;
using Wanderer.Models;
using Wanderer.Services.Config;

namespace Wanderer.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private object? _frameContent;

    [ObservableProperty]
    private bool _isPinned;

    [ObservableProperty]
    private FANavigationViewItemBase? _selectedNavigationViewItem;

    [ObservableProperty]
    private MainPageInfo? _selectedPageInfo;

    public MainViewModel(MainConfigHandler handler)
    {
        Config = handler.Data;
    }

    public MainConfigModel Config { get; }

    public bool IsWindows { get; } = OperatingSystem.IsWindows();
    public bool IsDesktop { get; } = App.IsDesktop;
    public ObservableCollection<FANavigationViewItemBase> NavigationViewItems { get; } = [];
    public ObservableCollection<FANavigationViewItemBase> NavigationViewFooterItems { get; } = [];
}