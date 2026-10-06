using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.ViewModels.MainPages;

namespace Wanderer.Views.MainPages;

[MainPageInfo("主页", "home", "\uE994")]
public partial class HomePage : UserControl
{
    public HomePage()
    {
        DataContext = this;
        InitializeComponent();
    }

    public HomePageViewModel ViewModel { get; } = IAppHost.GetService<HomePageViewModel>();

    private void GoAttendancePageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainView.Current?.SelectNavigationItemById("attendance");
    }

    private void ButtonRefresh_OnClick(object? sender, RoutedEventArgs e)
    {
        AttendanceViewer.RefreshData();
    }
}