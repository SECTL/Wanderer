using System;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Shared.Models;

using OneDayAttendanceStatus = OneDayAttendanceStatus;

public interface IProfileModel
{
    public Profile.Profile Profile { get; }
    public ObservableDictionary<DateOnly, OneDayAttendanceStatus> Statuses { get; }
}