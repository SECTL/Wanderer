using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Extensions;
using WandererAttendance.Services.Config;
using WandererAttendance.Shared.ComponentModels;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.ViewModels.MainPages;

public partial class AttendancePageViewModel : ObservableRecipient
{
    public ProfileConfigHandler ProfileConfigHandler { get; }
    
    public DateOnly TodayDate { get; } = DateOnly.FromDateTime(DateTime.Now);
    [ObservableProperty] private string _searchText = string.Empty;
    public ObservableDictionary<Guid, Person> Persons { get; } = [];
    
    public AttendancePageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
        Persons.AddRange(ProfileConfigHandler.Data.Profile.Persons);
    }
}