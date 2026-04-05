using System.Collections.ObjectModel;
using Wanderer.Attributes;

namespace Wanderer.Services;

public static class MainPagesRegistryService
{
    public static ObservableCollection<MainPageInfo> Items { get; } = [];
    public static ObservableCollection<MainPageInfo> FooterItems { get; } = [];
}