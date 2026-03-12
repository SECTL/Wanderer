using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WandererAttendance.Shared.ComponentModels;

namespace WandererAttendance.Shared.Models.Profile;

public partial class OneDayAttendanceStatus : ObservableRecipient
{
    [ObservableProperty] private ObservableDictionary<Guid, AttendanceStatus> _persons = [];
}