using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Extensions;
using Wanderer.ViewModels.MainPages;

namespace Wanderer.Views.MainPages;

[MainPageInfo("历史记录", "history", "\uE990", true, true)]
public partial class HistoryPage : UserControl
{
    public HistoryPage()
    {
        DataContext = this;
        InitializeComponent();
    }

    public HistoryPageViewModel ViewModel { get; } = IAppHost.GetService<HistoryPageViewModel>();

    private void ButtonRefresh_OnClick(object? sender, RoutedEventArgs e)
    {
        MainView.Current?.SelectNavigationItemById("history");
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