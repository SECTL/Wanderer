using System;
using WandererAttendance.Shared.ComponentModels;
using OneDayAttendanceStatus = WandererAttendance.Shared.Models.Profile.OneDayAttendanceStatus;

namespace WandererAttendance.Shared.Models;

public interface IProfileModel
{
    public Profile.Profile Profile { get; }
    public ObservableDictionary<DateOnly, OneDayAttendanceStatus> Statuses { get; }
}