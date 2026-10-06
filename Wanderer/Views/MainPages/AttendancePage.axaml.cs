using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Attributes;
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // 页面停留期间可能跨过零点，每次进入页面都重新解析当天日期。
        ViewModel.RefreshToday();
        Editor.Model.IsDirtyChanged += OnIsDirtyChanged;
        ViewModel.HasUnsavedChanges = Editor.Model.IsDirty;
        Editor.Model.SearchText = ViewModel.SearchText;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // 离开页面时把尚未落盘的改动写入档案。
        Editor.Model.Flush();
        Editor.Model.IsDirtyChanged -= OnIsDirtyChanged;
        ViewModel.HasUnsavedChanges = false;
    }

    private void OnIsDirtyChanged(bool isDirty)
    {
        ViewModel.HasUnsavedChanges = isDirty;
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var search = (sender as TextBox)?.Text ?? ViewModel.SearchText;
        if (search == Editor.Model.SearchText) return;

        Editor.Model.SearchText = search;
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Model.Flush();
        ViewModel.ProfileConfigHandler.Save();
        ViewModel.HasUnsavedChanges = false;
        this.ShowSuccessToast("考勤已保存。");
    }
}
