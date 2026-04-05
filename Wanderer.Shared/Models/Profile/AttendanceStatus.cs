using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wanderer.Shared.Models.Profile;

public partial class AttendanceStatus : ObservableRecipient
{
    [ObservableProperty] private ObservableCollection<Guid> _statuses = [];
}