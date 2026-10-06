using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Abstraction;
using Wanderer.Services.Config;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models;

public partial class PersonWithStatus : ObservableObject, IDisposable
{
    private readonly AttendanceStatus _attendanceStatus;
    private readonly OneDayAttendanceStatus _oneDayAttendanceStatus;

    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private Person _person;

    [ObservableProperty]
    private ObservableCollection<Guid> _statuses;

    private bool _tmpStatusFlag;

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

    private ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();

    public void Dispose()
    {
        Statuses.CollectionChanged -= StatusesOnCollectionChanged;
        GC.SuppressFinalize(this);
    }

    private void StatusesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_tmpStatusFlag) return;

        _oneDayAttendanceStatus.Persons[Guid] = _attendanceStatus;
        _tmpStatusFlag = false;
    }
}