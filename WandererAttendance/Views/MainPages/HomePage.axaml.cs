using Avalonia.Controls;
using Avalonia.Interactivity;
using WandererAttendance.Abstraction;
using WandererAttendance.Attributes;
using WandererAttendance.Services.Config;
using WandererAttendance.ViewModels.MainPages;

namespace WandererAttendance.Views.MainPages;

[MainPageInfo("主页", "home", "\uE994")]
public partial class HomePage : UserControl
{
    public HomePageViewModel ViewModel { get; } = IAppHost.GetService<HomePageViewModel>();
    
    public HomePage()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void GoAttendancePageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var view = IAppHost.GetService<MainView>();
        view.SelectNavigationItemById("attendance");
    }

    private void ButtonRefresh_OnClick(object? sender, RoutedEventArgs e)
    {
        AttendanceViewer.RefreshData();
    }
}