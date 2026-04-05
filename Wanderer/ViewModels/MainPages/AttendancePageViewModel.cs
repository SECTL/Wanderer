using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Services.Config;
using Wanderer.Extensions;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

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