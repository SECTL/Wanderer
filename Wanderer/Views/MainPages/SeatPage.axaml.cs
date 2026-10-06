using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Wanderer.Abstraction;
using Wanderer.Attributes;
using Wanderer.Helpers.UI;
using Wanderer.Icons;
using Wanderer.Services.Config;
using Wanderer.Shared;
using Wanderer.Shared.Models.Profile;
using Wanderer.ViewModels.MainPages;

namespace Wanderer.Views.MainPages;

[MainPageInfo("座位", "seat", FluentIcons.SeatRegular, true, true)]
public partial class SeatPage : UserControl
{
    private bool _isSyncingZoom;

    public SeatPage()
    {
        DataContext = this;
        InitializeComponent();

        ViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        ViewModel.LayoutReset += ViewModel_OnLayoutReset;
        ViewModel.SetupModeRejected += ViewModel_OnSetupModeRejected;
        ViewModel.DroppedOccupiedSeats += ViewModel_OnDroppedOccupiedSeats;

        // 座位区宽度变化时重算统一的座位尺寸（窗口越宽座位越大，并夹在最小/最大之间）。
        SeatZoom.SizeChanged += SeatZoom_OnSizeChanged;
    }

    public SeatPageViewModel ViewModel { get; } = IAppHost.GetService<SeatPageViewModel>();

    private ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ProfileConfigHandler.Data.Profile.Persons.CollectionChanged += Persons_OnCollectionChanged;

        ViewModel.Load();
        ViewModel.UpdateAvailableWidth(SeatZoom.Bounds.Width);
        ResetZoom();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        ProfileConfigHandler.Data.Profile.Persons.CollectionChanged -= Persons_OnCollectionChanged;

        // 离开页面时把尚未落盘的考勤改动写入档案。
        ViewModel.Flush();
    }

    /// <summary>
    ///     视图状态变化：缩放百分比同步给 PanAndZoom。
    /// </summary>
    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SeatPageViewModel.ZoomPercent))
        {
            ApplyZoom();
        }
    }

    private void ViewModel_OnLayoutReset(object? sender, EventArgs e)
    {
        ResetZoom();
    }

    private void SeatZoom_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 0)
        {
            ViewModel.UpdateAvailableWidth(e.NewSize.Width);
        }
    }

    private void ViewModel_OnSetupModeRejected(object? sender, EventArgs e)
    {
        this.ShowWarningToast("请先指定行与列，或导入座位表。");
    }

    private void ViewModel_OnDroppedOccupiedSeats(int count)
    {
        this.ShowWarningToast($"有 {count} 个已安排人员的座位因行列数减小被移除。");
    }

    private void Persons_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ViewModel.RefreshPersonChoices();
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        var hasChanges = ViewModel.HasUnsavedChanges;
        ViewModel.Flush();
        this.ShowSuccessToast(hasChanges ? "考勤已保存。" : "没有需要保存的改动。");
    }

    private async void ButtonImportSheet_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择座位表文件",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("所有支持格式") { Patterns = ["*.xlsx", "*.csv"] },
                new FilePickerFileType("Excel 文件") { Patterns = ["*.xlsx"] },
                new FilePickerFileType("CSV 文件") { Patterns = ["*.csv"] }
            ]
        });

        if (files.Count == 0)
        {
            return;
        }

        var file = files[0];
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (extension is not (".xlsx" or ".csv"))
        {
            this.ShowErrorToast("不支持的文件格式，请使用 .xlsx 或 .csv（暂不支持 .xls）。");
            return;
        }

        try
        {
            await using var stream = await file.OpenReadAsync();
            var sheet = await SheetLoader.LoadAsync(stream, extension);
            var result = ViewModel.ApplyImportedSheet(sheet);

            if (result.Rows == 0 || result.Columns == 0)
            {
                this.ShowWarningToast("座位表是空的，没有可导入的座位。");
                return;
            }

            var message = $"已应用 {result.Rows}×{result.Columns} 座位表：匹配 {result.Matched} 人，{result.Empty} 个座位为空。";
            if (result.Truncated)
            {
                message += $"（超出 {SeatLayout.MaxCount} 行/列的部分已截断）";
            }

            if (result.Skipped > 0)
            {
                this.ShowWarningToast($"{message}{result.Skipped} 个姓名未匹配已跳过，请先在「档案」页导入名单。");
            }
            else
            {
                this.ShowSuccessToast(message);
            }
        }
        catch (Exception exception)
        {
            this.ShowErrorToast("导入座位表失败", exception);
        }
    }

    /// <summary>
    ///     缩放变化（滚轮、捏合、双击）后同步右下角滚动条。
    /// </summary>
    private void SeatZoom_OnZoomChanged(object? sender, ZoomChangedEventArgs e)
    {
        if (_isSyncingZoom)
        {
            return;
        }

        _isSyncingZoom = true;
        ViewModel.ZoomPercent = Math.Round(e.ZoomX * 100);
        _isSyncingZoom = false;
    }

    private void ApplyZoom()
    {
        if (_isSyncingZoom)
        {
            return;
        }

        _isSyncingZoom = true;
        var zoom = Math.Clamp(ViewModel.ZoomPercent / 100.0, 0.25, 3.0);
        SeatZoom.CenterOn(SeatZoom.GetVisibleContentBounds().Center, zoom, false);
        _isSyncingZoom = false;
    }

    private void ResetZoom()
    {
        // 等布局完成后再复位，否则会按旧的尺寸居中。
        Dispatcher.UIThread.Post(() =>
        {
            SeatZoom.ResetMatrix(true);
            _isSyncingZoom = true;
            ViewModel.ZoomPercent = 100;
            _isSyncingZoom = false;
        }, DispatcherPriority.Background);
    }
}
