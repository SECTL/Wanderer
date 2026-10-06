using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Kernel;
using Wanderer.Abstraction;
using Wanderer.Models;
using Wanderer.Services.Config;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Controls;

public partial class AttendanceDayControl : UserControl
{
    public static readonly StyledProperty<DateTime> DateProperty =
        AvaloniaProperty.Register<AttendanceDayControl, DateTime>(nameof(Date));

    public static readonly StyledProperty<string> SimpleTextProperty =
        AvaloniaProperty.Register<AttendanceDayControl, string>(nameof(SimpleText), string.Empty);

    public static readonly StyledProperty<bool> ShowSimpleTextProperty =
        AvaloniaProperty.Register<AttendanceDayControl, bool>(nameof(ShowSimpleText));

    static AttendanceDayControl()
    {
        DateProperty.Changed.AddClassHandler<AttendanceDayControl>((x, e) => x.OnDateChanged(e));
    }

    public AttendanceDayControl()
    {
        InitializeComponent();
    }

    public DateTime Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public string SimpleText
    {
        get => GetValue(SimpleTextProperty);
        set => SetValue(SimpleTextProperty, value);
    }

    public bool ShowSimpleText
    {
        get => GetValue(ShowSimpleTextProperty);
        set => SetValue(ShowSimpleTextProperty, value);
    }

    public ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();
    public ObservableCollection<StatusAndCount> Data { get; } = [];

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ShowSimpleText = e.NewSize.Height <= 96 || e.NewSize.Width <= 96;
    }

    private void OnDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DateTime)
        {
            RefreshData();
        }
    }

    public void RefreshData()
    {
        Data.Clear();
        var date = DateOnly.FromDateTime(Date);
        var config = ProfileConfigHandler.Data;

        // 读取当天的状态；记录缺失、缺少某个人员或存储值为 null 时按默认状态处理。
        var attendanceStatus = config.Statuses.GetValueOrDefault(date);
        var defaultStatus = ProfileConfigHandler.CreateDefaultStatus(config.Profile);
        var personStatuses = config.Profile.Persons.ToDictionary(
            person => person.Key,
            person => attendanceStatus?.Persons.GetValueOrDefault(person.Key) ?? defaultStatus);

        // 统计数据
        Data.AddRange(config.Profile.Statuses
                            .Select(s => new StatusAndCount
                            {
                                Status = s.Value,
                                Count = personStatuses.Count(p => p.Value.Statuses.Contains(s.Key)),
                                Persons = [] // 当前控件无需显示详细人员
                            }));

        // 当天有考勤记录时，补充"无状态"（一个状态都没有的人员）
        if (attendanceStatus is not null)
        {
            var noStatusCount = personStatuses.Count(p => p.Value.Statuses.Count == 0);
            if (noStatusCount > 0)
            {
                Data.Add(new StatusAndCount
                {
                    Status = StatusAndCount.NoStatus,
                    Count = noStatusCount,
                    Persons = []
                });
            }
        }

        // 简略文本
        if (attendanceStatus is null)
        {
            SimpleText = "无记录";
            return;
        }

        // 只有"无状态"一种情况时优先显示
        if (Data.Count == 1 && Data[0].Status == StatusAndCount.NoStatus)
        {
            SimpleText = $"{StatusAndCount.NoStatusTitle} {Data[0].Count} 人";
            return;
        }

        var firstStatus = config.Profile.Statuses.FirstOrOptional(s => s.Value.IsDefault);
        if (!firstStatus.HasValue)
        {
            firstStatus = config.Profile.Statuses.FirstOrOptional(_ => true);
        }

        if (!firstStatus.HasValue)
        {
            SimpleText = "无状态";
            return;
        }

        var count = personStatuses.Count(p => p.Value.Statuses.Contains(firstStatus.Value.Key));
        SimpleText = $"{firstStatus.Value.Value.Name} {count} 人";
    }
}
