using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Models;
using WandererAttendance.Models.Profile;
using WandererAttendance.Services.Config;

namespace WandererAttendance.ViewModels.MainPages;

public partial class HomePageViewModel : ObservableRecipient
{
    public DateOnly TodayDate { get; } = DateOnly.FromDateTime(DateTime.Now);
}