using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wanderer.ViewModels.MainPages;

public partial class HomePageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private DateOnly _currentDate = DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    ///     重新解析当前日期。页面每次加载时调用，保证跨零点后指向新的一天。
    /// </summary>
    public void RefreshCurrentDate()
    {
        CurrentDate = DateOnly.FromDateTime(DateTime.Now);
    }
}
