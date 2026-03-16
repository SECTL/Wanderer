using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using WandererAttendance.Abstraction;
using WandererAttendance.Attributes;
using WandererAttendance.Extensions;
using WandererAttendance.Helpers.UI;
using WandererAttendance.Models;
using WandererAttendance.Models.Ranking;
using WandererAttendance.ViewModels.MainPages;

namespace WandererAttendance.Views.MainPages;

[MainPageInfo("排行榜", "ranking", "\uE3E0", true, true)]
public partial class RankingPage : UserControl
{
    public RankingPageViewModel ViewModel { get; } = IAppHost.GetService<RankingPageViewModel>();
    
    public RankingPage()
    {
        DataContext = this;
        InitializeComponent();

        PersonsDataGrid.Columns.AddRange(ViewModel.DataGridColumns);
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
    
    [RelayCommand]
    public void CopyStatusWithRanking(StatusWithRanking ranking)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        var text = ranking.Items
            .Aggregate(
                $"{ranking.Status.Name}：{ranking.Items.Count} 人上榜",
                (current, item) => current + $"\n{item.Person.Name} {item.Count} 次");

        topLevel.Clipboard.SetTextAsync(text).Wait();
        this.ShowSuccessToast("复制成功。");
    }
}