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
    public MainConfigModel Config { get; }

    public bool IsWindows { get; } = OperatingSystem.IsWindows();
    public bool IsDesktop { get; } = App.IsDesktop;
    [ObservableProperty] private bool _isPinned = false;
    
    [ObservableProperty] private object? _frameContent;
    [ObservableProperty] private MainPageInfo? _selectedPageInfo = null;
    [ObservableProperty] private NavigationViewItemBase? _selectedNavigationViewItem = null;
    public ObservableCollection<NavigationViewItemBase> NavigationViewItems { get; } = [];
    public ObservableCollection<NavigationViewItemBase> NavigationViewFooterItems { get; } = [];

    public MainViewModel(MainConfigHandler handler)
    {
        Config = handler.Data;
    }
}