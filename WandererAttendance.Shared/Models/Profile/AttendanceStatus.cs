using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WandererAttendance.Shared.Models.Profile;

public partial class AttendanceStatus : ObservableRecipient
{
    [ObservableProperty] private ObservableCollection<Guid> _statuses = [];
}