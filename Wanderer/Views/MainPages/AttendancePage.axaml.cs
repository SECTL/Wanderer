using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Extensions;
using Wanderer.Helpers.UI;
using Wanderer.ViewModels.MainPages;

namespace Wanderer.Views.MainPages;

[MainPageInfo("考勤", "attendance", "\uE430", true, true)]
public partial class AttendancePage : UserControl
{
    public AttendancePage()
    {
        DataContext = this;
        InitializeComponent();
    }

    public AttendancePageViewModel ViewModel { get; } = IAppHost.GetService<AttendancePageViewModel>();

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

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ProfileConfigHandler.Save();
        this.ShowSuccessToast("已保存。");
    }
}