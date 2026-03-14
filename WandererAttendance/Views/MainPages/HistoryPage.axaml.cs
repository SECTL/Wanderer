using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WandererAttendance.Abstraction;
using WandererAttendance.Attributes;
using WandererAttendance.Extensions;
using WandererAttendance.Shared;
using WandererAttendance.ViewModels.MainPages;

namespace WandererAttendance.Views.MainPages;

[MainPageInfo("历史记录", "history", "\uE990", true, true)]
public partial class HistoryPage : UserControl
{
    public HistoryPageViewModel ViewModel { get; } = IAppHost.GetService<HistoryPageViewModel>();
    
    public HistoryPage()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void ButtonRefresh_OnClick(object? sender, RoutedEventArgs e)
    {
        var view = IAppHost.GetService<MainView>();
        view.SelectNavigationItemById("history");
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var search = ViewModel.SearchText;
        ViewModel.Persons.Clear();
        if (search == string.Empty)
        {
            ViewModel.Persons.AddRange(ViewModel.ProfileConfigHandler.Data.Profile.Persons);
            return;
        }
        
        ViewModel.Persons.AddRange(ViewModel.ProfileConfigHandler.Data.Profile.Persons
            .Where(person => person.Value.IsMatch(search)));
    }
}