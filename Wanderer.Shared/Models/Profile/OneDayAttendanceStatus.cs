using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Shared.ComponentModels;

namespace Wanderer.Shared.Models.Profile;

public partial class OneDayAttendanceStatus : ObservableRecipient
{
    [ObservableProperty] private ObservableDictionary<Guid, AttendanceStatus> _persons = [];
}