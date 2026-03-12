using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using DynamicData;
using WandererAttendance.Abstraction;
using WandererAttendance.Models;
using WandererAttendance.Services.Config;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Controls;

public partial class OneDayAttendanceViewer : UserControl
{
    public static readonly StyledProperty<DateTime> DateProperty =
        AvaloniaProperty.Register<AttendanceDayControl, DateTime>(nameof(Date));

    public DateTime Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }
    
    public ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();
    public ObservableCollection<StatusAndCount> Data { get; } = [];
    
    static OneDayAttendanceViewer()
    {
        DateProperty.Changed.AddClassHandler<OneDayAttendanceViewer>((x, e) => x.OnDateChanged(e));
    }

    public OneDayAttendanceViewer()
    {
        InitializeComponent();
        RefreshData();
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
        
        // 拉取数据
        var attendanceStatus = Utils.CopyObjectByJson(
            config.Statuses.GetValueOrDefault(date, new OneDayAttendanceStatus()));
        foreach (var kvp in config.Profile.Persons)
        {
            if (attendanceStatus.Persons.GetValueOrDefault(kvp.Key) != null) continue;

            var status = new AttendanceStatus();
            status.Statuses.AddRange(config.Profile.Statuses
                .Where(s => s.Value.IsDefault)
                .Select(s => s.Key));
            attendanceStatus.Persons[kvp.Key] = status;
        }
        
        // 统计数据
        Data.AddRange(config.Profile.Statuses
            .Select(s => new StatusAndCount
            {
                Status = s.Value,
                Count = config.Profile.Persons
                    .Count(p => attendanceStatus.Persons[p.Key].Statuses.Contains(s.Key)),
                Persons = config.Profile.Persons
                    .Where(p => attendanceStatus.Persons[p.Key].Statuses.Contains(s.Key))
                    .Select(p => p.Value)
                    .ToList()
            }));
    }
}