using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Abstraction;
using WandererAttendance.Services.Config;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Models;

public partial class PersonWithStatus : ObservableObject, IDisposable
{
    private ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();
    private OneDayAttendanceStatus _oneDayAttendanceStatus;
    private AttendanceStatus _attendanceStatus;
    
    private bool _tmpStatusFlag = false;
    
    [ObservableProperty] private Guid _guid;
    [ObservableProperty] private Person _person;
    [ObservableProperty] private ObservableCollection<Guid> _statuses;

    public PersonWithStatus(Guid guid, Person person, OneDayAttendanceStatus status)
    {
        Guid = guid;
        Person = person;
        _oneDayAttendanceStatus = status;

        var attendanceStatus = status.Persons.GetValueOrDefault(guid);
        if (attendanceStatus == null)
        {
            attendanceStatus = new AttendanceStatus();
            _tmpStatusFlag = true;
            foreach (var kvp in ProfileConfigHandler.Data.Profile.Statuses)
            {
                if (!kvp.Value.IsDefault) continue;
                attendanceStatus.Statuses.Add(kvp.Key);
            }
        }

        _attendanceStatus = attendanceStatus;
        Statuses = attendanceStatus.Statuses;
        Statuses.CollectionChanged += StatusesOnCollectionChanged;
    }

    private void StatusesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_tmpStatusFlag) return;
        
        _oneDayAttendanceStatus.Persons[Guid] = _attendanceStatus;
        _tmpStatusFlag = false;
    }

    public void Dispose()
    {
        Statuses.CollectionChanged -= StatusesOnCollectionChanged;
        GC.SuppressFinalize(this);
    }
}