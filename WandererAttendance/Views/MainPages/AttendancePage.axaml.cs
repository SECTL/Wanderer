using System;
using System.Linq;
using Avalonia.Controls;
using DynamicData;
using WandererAttendance.Abstraction;
using WandererAttendance.Attributes;
using WandererAttendance.Shared;
using WandererAttendance.ViewModels.MainPages;

namespace WandererAttendance.Views.MainPages;

[MainPageInfo("考勤", "attendance", "\uE430", false, true)]
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
        ViewModel.PersonSource.Clear();
        if (search == string.Empty)
        {
            ViewModel.PersonSource.AddRange(ViewModel.ProfileConfigHandler.Data.Profile.Persons);
            return;
        }

        const StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
        
        ViewModel.PersonSource.AddRange(ViewModel.ProfileConfigHandler.Data.Profile.Persons
            .Where(person =>
                person.Name.Contains(search, ignoreCase) 
                || person.Id.Contains(search, ignoreCase)
                || PinyinHelper.GetFullPinyinList(person.Name)
                    .Any(pinyin => pinyin.StartsWith(search, ignoreCase))
                || PinyinHelper.GetFirstPinyinList(person.Name)
                    .Any(pinyin => pinyin.StartsWith(search, ignoreCase))));
    }
}