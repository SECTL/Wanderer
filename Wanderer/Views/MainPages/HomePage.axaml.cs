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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // 页面停留期间可能跨过零点，每次进入页面都重新解析日期。
        ViewModel.RefreshCurrentDate();
        AttendanceViewer.RefreshData();
    }

    private void GoAttendancePageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainView.Current?.SelectNavigationItemById("attendance");
    }

    private void ButtonRefresh_OnClick(object? sender, RoutedEventArgs e)
    {
        AttendanceViewer.RefreshData();
    }
}