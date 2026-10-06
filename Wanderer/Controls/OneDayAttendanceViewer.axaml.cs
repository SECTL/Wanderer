using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Wanderer.Abstraction;
using Wanderer.Helpers.UI;
using Wanderer.Models;
using Wanderer.Services.Config;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Controls;

public partial class OneDayAttendanceViewer : UserControl
{
    public static readonly StyledProperty<DateOnly> DateProperty =
        AvaloniaProperty.Register<OneDayAttendanceViewer, DateOnly>(
            nameof(Date), DateOnly.FromDateTime(DateTime.Today));

    static OneDayAttendanceViewer()
    {
        DateProperty.Changed.AddClassHandler<OneDayAttendanceViewer>((x, e) => x.OnDateChanged(e));
    }

    public OneDayAttendanceViewer()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // 挂载到界面后按当前日期填充一次数据。此时所有有界属性已应用完毕。
        RefreshData();
    }

    public DateOnly Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();
    public ObservableCollection<StatusAndCount> Data { get; } = [];

    private void OnDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DateOnly)
        {
            RefreshData();
        }
    }

    public void RefreshData()
    {
        Data.Clear();
        var date = Date;
        var config = ProfileConfigHandler.Data;

        // 读取当天的状态；记录缺失、缺少某个人员或存储值为 null 时按默认状态处理。
        var attendanceStatus = config.Statuses.GetValueOrDefault(date);
        var personStatuses = config.Profile.Persons.ToDictionary(
            person => person.Key,
            person => attendanceStatus?.Persons.GetValueOrDefault(person.Key)
                       ?? ProfileConfigHandler.CreateDefaultStatus(config.Profile));

        // 统计数据
        Data.AddRange(config.Profile.Statuses
                            .Select(s => new StatusAndCount
                            {
                                Status = s.Value,
                                Count = personStatuses.Count(p => p.Value.Statuses.Contains(s.Key)),
                                Persons = personStatuses.Where(p => p.Value.Statuses.Contains(s.Key))
                                                        .Select(p => config.Profile.Persons[p.Key])
                                                        .ToList()
                            }));
    }

    [RelayCommand]
    public void CopyStatusAndCount(StatusAndCount statusAndCount)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        var text = statusAndCount.Persons
                                 .Aggregate(
                                     $"{statusAndCount.Status.Name}：{statusAndCount.Count} 人",
                                     (current, person) => current + $"\n{person.Name}");

        topLevel.Clipboard.SetTextAsync(text).Wait();
        this.ShowSuccessToast("复制成功。");
    }
}
