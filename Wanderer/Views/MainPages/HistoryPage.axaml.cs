using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Helpers.UI;
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        HistoryEditor.Model.SearchText = ViewModel.SearchText;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // 离开页面时把尚未落盘的改动写入档案。
        HistoryEditor.Model.Flush();
    }

    private void ButtonRefresh_OnClick(object? sender, RoutedEventArgs e)
    {
        // 只重新读取当前选择日期的记录，不重建页面。
        DayViewer.RefreshData();

        var date = ViewModel.PickerDate?.Date;
        if (date is { } picked)
        {
            Calendar.RefreshDate(picked);
        }
        else
        {
            Calendar.RefreshData();
        }

        this.ShowSuccessToast("已刷新。");
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var search = (sender as TextBox)?.Text ?? ViewModel.SearchText;
        if (search == HistoryEditor.Model.SearchText) return;

        HistoryEditor.Model.SearchText = search;
    }
}
