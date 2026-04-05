using System;
using Wanderer.Shared.ComponentModels;

namespace Wanderer.Shared.Models;

using OneDayAttendanceStatus = Profile.OneDayAttendanceStatus;

public interface IProfileModel
{
    public Profile.Profile Profile { get; }
    public ObservableDictionary<DateOnly, OneDayAttendanceStatus> Statuses { get; }
}