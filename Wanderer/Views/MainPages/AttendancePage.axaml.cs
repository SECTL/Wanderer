using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.ViewModels.MainPages;
using Wanderer.Extensions;
using Wanderer.Helpers.UI;

namespace Wanderer.Views.MainPages;

[MainPageInfo("考勤", "attendance", "\uE430", true, true)]
public partial class AttendancePage : UserControl
{
    public AttendancePageViewModel ViewModel { get; } = IAppHost.GetService<AttendancePageViewModel>();
    
    public AttendancePage()
    {
        DataContext = this;
        InitializeComponent();
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

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ProfileConfigHandler.Save();
        this.ShowSuccessToast("已保存。");
    }
}