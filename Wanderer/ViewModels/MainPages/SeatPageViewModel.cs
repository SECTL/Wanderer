using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Extensions;
using Wanderer.Services.Config;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

/// <summary>
///     导入座位表的结果，用于提示用户匹配情况。
/// </summary>
public record SeatImportResult(int Rows, int Columns, int Matched, int Skipped, int Empty, bool Truncated);

/// <summary>
///     座位页。设置模式负责排版（行列数、导入）与指派（是否有人、选人），
///     正常模式显示姓名、编号与考勤控件。
/// </summary>
public partial class SeatPageViewModel : ObservableRecipient
{
    /// <summary>
    ///     基准座位宽度，同时决定高宽比。
    /// </summary>
    public const double SeatWidth = 192;

    /// <summary>
    ///     基准座位高度，同时决定高宽比。
    /// </summary>
    public const double SeatHeight = 96;

    /// <summary>
    ///     座位宽度下限：可用宽度不足时缩到这里，再不够就交给平移/缩放。
    /// </summary>
    public const double MinSeatWidth = 128;

    /// <summary>
    ///     座位宽度上限：可用宽度充裕时最多放大到这里。
    /// </summary>
    public const double MaxSeatWidth = 232;

    /// <summary>
    ///     座位高度下限：内容（姓名/编号/考勤状态）至少需要这么高。
    /// </summary>
    public const double MinSeatHeight = 104;

    /// <summary>
    ///     座位高度上限。
    /// </summary>
    public const double MaxSeatHeight = 160;

    /// <summary>
    ///     座位卡片外边距。
    /// </summary>
    public const double SeatMargin = 4;

    /// <summary>
    ///     座位网格外边距（与 ItemsControl 的 Margin 一致）。
    /// </summary>
    public const double GridMargin = 64;

    private double _availableWidth;
    private bool _isApplyingLayout;
    private bool _isLoading;
    private bool _isRevertingSetupMode;

    public SeatPageViewModel(MainConfigHandler mainConfigHandler, ProfileConfigHandler profileConfigHandler)
    {
        MainConfigHandler = mainConfigHandler;
        ProfileConfigHandler = profileConfigHandler;
    }

    public MainConfigHandler MainConfigHandler { get; }

    public ProfileConfigHandler ProfileConfigHandler { get; }

    public ObservableCollection<SeatItemViewModel> Seats { get; } = [];

    public ObservableCollection<PersonChoice> PersonChoices { get; } = [];

    /// <summary>
    ///     选人框的过滤方法（姓名/编号/拼音）。
    /// </summary>
    public AutoCompleteFilterPredicate<object> PersonFilter { get; } = FilterPerson;

    public SeatLayout Layout => ProfileConfigHandler.Data.Profile.SeatLayout;

    [ObservableProperty]
    private bool _isSetupMode;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private int _rows;

    [ObservableProperty]
    private int _columns;

    [ObservableProperty]
    private DateTime? _pickerDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ZoomPercentText))]
    private double _zoomPercent = 100;

    public string ZoomPercentText => $"{ZoomPercent:0}%";

    /// <summary>
    ///     当前编辑的日期。
    /// </summary>
    public DateOnly SelectedDate =>
        PickerDate is { } picked ? DateOnly.FromDateTime(picked) : DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    ///     座位卡片统一宽度：由「座位区可用宽度 ÷ 列数」算出后夹到
    ///     [<see cref="MinSeatWidth" />, <see cref="MaxSeatWidth" />]，所有座位共用同一个值。
    /// </summary>
    [ObservableProperty]
    private double _seatItemWidth = SeatWidth - SeatMargin * 2;

    /// <summary>
    ///     座位卡片统一高度：按基准高宽比跟随宽度，并夹在
    ///     [<see cref="MinSeatHeight" />, <see cref="MaxSeatHeight" />] 之间，所有座位等高。
    /// </summary>
    [ObservableProperty]
    private double _seatItemHeight = SeatHeight - SeatMargin * 2;

    /// <summary>
    ///     网格总宽度：列数 × (座位宽度 + 两侧外边距)，即 WrapPanel 的换行点。
    /// </summary>
    public double GridWidth => Columns * (SeatItemWidth + SeatMargin * 2);

    /// <summary>
    ///     网格总高度：行数 × (座位高度 + 上下外边距)。
    /// </summary>
    public double GridHeight => Rows * (SeatItemHeight + SeatMargin * 2);

    /// <summary>
    ///     座位卡片外边距。
    /// </summary>
    public Thickness SeatThickness => new(SeatMargin);

    /// <summary>
    ///     座位网格外边距。
    /// </summary>
    public Thickness GridThickness => new(GridMargin);

    /// <summary>
    ///     座位区可用宽度变化时重算统一的座位尺寸。窗口越宽座位越大，
    ///     但不会小于 <see cref="MinSeatWidth" /> / <see cref="MinSeatHeight" />，
    ///     也不会大于 <see cref="MaxSeatWidth" /> / <see cref="MaxSeatHeight" />。
    /// </summary>
    public void UpdateAvailableWidth(double viewportWidth)
    {
        _availableWidth = viewportWidth;

        var columns = Math.Max(1, Columns);
        var usable = viewportWidth - GridMargin * 2;
        if (usable <= 0)
        {
            return;
        }

        var width = Math.Clamp(usable / columns - SeatMargin * 2, MinSeatWidth, MaxSeatWidth);
        var height = Math.Clamp(width * SeatHeight / SeatWidth, MinSeatHeight, MaxSeatHeight);

        SeatItemWidth = Math.Round(width);
        SeatItemHeight = Math.Round(height);
    }

    /// <summary>
    ///     座位数量或布局发生变化，视图需要重置平移/缩放视图。
    /// </summary>
    public event EventHandler? LayoutReset;

    /// <summary>
    ///     未配置座位表时试图离开设置模式。
    /// </summary>
    public event EventHandler? SetupModeRejected;

    /// <summary>
    ///     被裁掉的、原本有人的座位数量。
    /// </summary>
    public event Action<int>? DroppedOccupiedSeats;

    /// <summary>
    ///     进入页面时载入布局。从未配置过时进入设置模式，并先在内存中铺好空座位，
    ///     用户做出改动之前不写入档案。
    /// </summary>
    public void Load()
    {
        RefreshPersonChoices();

        var layout = Layout;
        // 从未配置过、或所有座位都空着时，进入设置模式。
        var firstRun = !layout.IsConfigured || layout.Seats.All(seat => seat.PersonKey is null);

        _isApplyingLayout = true;
        try
        {
            Rows = Math.Clamp(layout.Rows > 0 ? layout.Rows : 6, 1, SeatLayout.MaxCount);
            Columns = Math.Clamp(layout.Columns > 0 ? layout.Columns : 8, 1, SeatLayout.MaxCount);

            if (layout.Seats.Count != Rows * Columns)
            {
                layout.Resize(Rows, Columns, out _);
            }
        }
        finally
        {
            _isApplyingLayout = false;
        }

        BuildSeats();

        _isLoading = true;
        IsSetupMode = firstRun;
        _isLoading = false;

        RefreshAttendanceRows();
    }

    /// <summary>
    ///     离开页面时把未落盘的考勤改动写入档案。
    /// </summary>
    public void Flush()
    {
        if (!HasUnsavedChanges)
        {
            return;
        }

        ProfileConfigHandler.Save();
        HasUnsavedChanges = false;
    }

    /// <summary>
    ///     布局类改动（行列数、指派、导入）立即落盘。
    /// </summary>
    public void SaveLayout()
    {
        ProfileConfigHandler.Save();
        HasUnsavedChanges = false;
    }

    /// <summary>
    ///     依据当前日期重建全部座位的考勤行；设置模式下不建立。
    /// </summary>
    public void RefreshAttendanceRows()
    {
        if (IsSetupMode)
        {
            ClearAttendanceRows();
            return;
        }

        var date = SelectedDate;
        foreach (var seat in Seats)
        {
            seat.RefreshAttendance(date);
        }
    }

    /// <summary>
    ///     重建选人框的候选人员。
    /// </summary>
    public void RefreshPersonChoices()
    {
        PersonChoices.Clear();
        foreach (var kvp in ProfileConfigHandler.Data.Profile.Persons)
        {
            PersonChoices.Add(new PersonChoice
            {
                Key = kvp.Key,
                Person = kvp.Value
            });
        }
    }

    public PersonChoice? FindPerson(Guid? key)
    {
        return key is not { } id ? null : PersonChoices.FirstOrDefault(choice => choice.Key == id);
    }

    /// <summary>
    ///     把选人框中的文本解析为档案中的已有人员。同名多人时优先返回未被其它座位占用的人。
    /// </summary>
    public PersonChoice? ResolvePerson(string text, SeatItemViewModel? requester)
    {
        var matches = PersonChoices
                      .Where(choice => string.Equals(choice.Name, text, StringComparison.CurrentCultureIgnoreCase) ||
                                       (!string.IsNullOrWhiteSpace(choice.Id) &&
                                        string.Equals(choice.Id, text, StringComparison.CurrentCultureIgnoreCase)))
                      .ToList();

        if (matches.Count <= 1)
        {
            return matches.FirstOrDefault();
        }

        var taken = Seats
                    .Where(seat => !ReferenceEquals(seat, requester))
                    .Select(seat => seat.Occupant?.Key)
                    .Where(key => key is not null)
                    .ToHashSet();

        return matches.FirstOrDefault(choice => !taken.Contains(choice.Key)) ?? matches[0];
    }

    /// <summary>
    ///     某个座位的考勤状态发生变化：把状态挂到当前日期的记录上，并标记待保存。
    /// </summary>
    public void OnSeatStatusChanged(SeatItemViewModel seat)
    {
        if (seat.Model.PersonKey is not { } personKey || seat.Attendance is not { } attendance)
        {
            return;
        }

        var statuses = ProfileConfigHandler.Data.Statuses;
        var date = SelectedDate;
        var day = statuses.GetValueOrDefault(date);
        if (day is null)
        {
            day = new OneDayAttendanceStatus();
            statuses[date] = day;
        }

        day.Persons[personKey] = attendance.StoredStatus;
        HasUnsavedChanges = true;
    }

    /// <summary>
    ///     应用导入的座位表（A1 起全部视为座位，不识别表头）。只匹配档案中已有人员，
    ///     未匹配的姓名会被跳过。
    /// </summary>
    public SeatImportResult ApplyImportedSheet(IReadOnlyList<IReadOnlyList<string>> sheet)
    {
        var rows = sheet.Count;
        while (rows > 0 && sheet[rows - 1].All(string.IsNullOrWhiteSpace))
        {
            rows--;
        }

        var columns = 0;
        for (var row = 0; row < rows; row++)
        {
            var line = sheet[row];
            for (var column = line.Count - 1; column >= 0; column--)
            {
                if (string.IsNullOrWhiteSpace(line[column]))
                {
                    continue;
                }

                columns = Math.Max(columns, column + 1);
                break;
            }
        }

        if (rows == 0 || columns == 0)
        {
            return new SeatImportResult(0, 0, 0, 0, 0, false);
        }

        var truncated = rows > SeatLayout.MaxCount || columns > SeatLayout.MaxCount;
        rows = Math.Min(rows, SeatLayout.MaxCount);
        columns = Math.Min(columns, SeatLayout.MaxCount);

        _isApplyingLayout = true;
        try
        {
            Layout.Resize(rows, columns, out _);
            Rows = Layout.Rows;
            Columns = Layout.Columns;
        }
        finally
        {
            _isApplyingLayout = false;
        }

        var matched = 0;
        var skipped = 0;
        var used = new HashSet<Guid>();

        for (var row = 0; row < rows; row++)
        {
            var line = sheet[row];
            for (var column = 0; column < columns; column++)
            {
                var text = (column < line.Count ? line[column] : string.Empty)?.Trim() ?? string.Empty;
                var seat = Layout.Seats[row * columns + column];

                if (text.Length == 0)
                {
                    seat.PersonKey = null;
                    continue;
                }

                var choice = MatchImportedName(text, used);
                if (choice is null)
                {
                    seat.PersonKey = null;
                    skipped++;
                    continue;
                }

                seat.PersonKey = choice.Key;
                used.Add(choice.Key);
                matched++;
            }
        }

        var empty = Layout.Seats.Count(seat => seat.PersonKey is null);

        BuildSeats();
        SaveLayout();
        LayoutReset?.Invoke(this, EventArgs.Empty);

        return new SeatImportResult(rows, columns, matched, skipped, empty, truncated);
    }

    partial void OnRowsChanged(int value)
    {
        OnPropertyChanged(nameof(GridWidth));
        OnPropertyChanged(nameof(GridHeight));
        ApplyLayoutSize();
    }

    partial void OnColumnsChanged(int value)
    {
        OnPropertyChanged(nameof(GridWidth));
        OnPropertyChanged(nameof(GridHeight));
        ApplyLayoutSize();
    }

    partial void OnPickerDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(SelectedDate));
        Flush();
        RefreshAttendanceRows();
    }

    partial void OnIsSetupModeChanged(bool value)
    {
        if (_isLoading)
        {
            if (value)
            {
                ClearAttendanceRows();
            }

            return;
        }

        if (_isRevertingSetupMode)
        {
            return;
        }

        if (value)
        {
            ClearAttendanceRows();
            return;
        }

        if (Layout.Seats.Count == 0)
        {
            // 还没有指定行/列或导入座位表，不允许进入正常模式。
            _isRevertingSetupMode = true;
            IsSetupMode = true;
            _isRevertingSetupMode = false;
            SetupModeRejected?.Invoke(this, EventArgs.Empty);
            return;
        }

        SaveLayout();
        RefreshAttendanceRows();
    }

    private static bool FilterPerson(string? search, object item)
    {
        if (item is not PersonChoice choice)
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(search) || choice.Person.IsMatch(search);
    }

    private PersonChoice? MatchImportedName(string text, HashSet<Guid> used)
    {
        var candidates = PersonChoices
                         .Where(choice => string.Equals(choice.Name, text, StringComparison.CurrentCultureIgnoreCase))
                         .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates.FirstOrDefault(choice => !used.Contains(choice.Key)) ?? candidates[0];
    }

    private void ApplyLayoutSize()
    {
        if (_isApplyingLayout)
        {
            return;
        }

        _isApplyingLayout = true;
        try
        {
            Layout.Resize(Rows, Columns, out var droppedOccupied);
            Rows = Layout.Rows;
            Columns = Layout.Columns;

            if (droppedOccupied > 0)
            {
                DroppedOccupiedSeats?.Invoke(droppedOccupied);
            }
        }
        finally
        {
            _isApplyingLayout = false;
        }

        // 列数变了，统一座位尺寸要按同样的可用宽度重算。
        UpdateAvailableWidth(_availableWidth);
        BuildSeats();
        SaveLayout();
        LayoutReset?.Invoke(this, EventArgs.Empty);
    }

    partial void OnSeatItemWidthChanged(double value)
    {
        OnPropertyChanged(nameof(GridWidth));
    }

    partial void OnSeatItemHeightChanged(double value)
    {
        OnPropertyChanged(nameof(GridHeight));
    }

    private void BuildSeats()
    {
        foreach (var seat in Seats)
        {
            seat.Dispose();
        }

        Seats.Clear();

        foreach (var seat in Layout.Seats)
        {
            Seats.Add(new SeatItemViewModel(seat, this));
        }

        OnPropertyChanged(nameof(GridWidth));
        OnPropertyChanged(nameof(GridHeight));
    }

    private void ClearAttendanceRows()
    {
        foreach (var seat in Seats)
        {
            seat.ClearAttendance();
        }
    }
}
