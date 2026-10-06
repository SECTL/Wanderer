using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Extensions;
using Wanderer.Services.Config;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

public partial class AttendancePageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    public AttendancePageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
        Persons.AddRange(ProfileConfigHandler.Data.Profile.Persons);
    }

    public ProfileConfigHandler ProfileConfigHandler { get; }

    public DateOnly TodayDate { get; } = DateOnly.FromDateTime(DateTime.Now);
    public ObservableDictionary<Guid, Person> Persons { get; } = [];
}