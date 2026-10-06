using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Services.Config;

namespace Wanderer.ViewModels.MainPages;

public partial class AttendancePageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    public AttendancePageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
    }

    public ProfileConfigHandler ProfileConfigHandler { get; }

    /// <summary>
    ///     当前考勤日期。考勤页面只记录当天，不支持切换日期。
    /// </summary>
    [ObservableProperty]
    private DateOnly _todayDate = DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    ///     重新解析当天日期，页面每次加载时调用。
    /// </summary>
    public void RefreshToday()
    {
        TodayDate = DateOnly.FromDateTime(DateTime.Now);
    }
}
