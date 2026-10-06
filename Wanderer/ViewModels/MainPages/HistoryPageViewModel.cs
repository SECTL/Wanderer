using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Services.Config;

namespace Wanderer.ViewModels.MainPages;

public partial class HistoryPageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    ///     日期选择框与日历视图共用的选择值。
    /// </summary>
    [ObservableProperty]
    private DateTime? _pickerDate = DateTime.Today;

    [ObservableProperty]
    private int _selectedPage;

    [ObservableProperty]
    private int _selectedSubPage;

    public HistoryPageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
    }

    public ProfileConfigHandler ProfileConfigHandler { get; }

    /// <summary>
    ///     当前查看的日期。由 <see cref="PickerDate" /> 派生，供看板与考勤编辑器使用。
    /// </summary>
    public DateOnly SelectedDate =>
        PickerDate is { } picked ? DateOnly.FromDateTime(picked) : DateOnly.FromDateTime(DateTime.Now);

    partial void OnPickerDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(SelectedDate));
    }
}
