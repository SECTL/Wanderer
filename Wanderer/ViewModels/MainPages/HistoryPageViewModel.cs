using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Extensions;
using Wanderer.Services.Config;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

public partial class HistoryPageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private int _selectedPage;

    [ObservableProperty]
    private int _selectedSubPage;

    public HistoryPageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
        Persons.AddRange(ProfileConfigHandler.Data.Profile.Persons);
    }

    public ProfileConfigHandler ProfileConfigHandler { get; }
    public ObservableDictionary<Guid, Person> Persons { get; } = [];
}