using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Services.Config;
using Wanderer.Extensions;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

public partial class HistoryPageViewModel : ObservableRecipient
{
    public ProfileConfigHandler ProfileConfigHandler { get; }
    
    [ObservableProperty] private int _selectedPage = 0;
    [ObservableProperty] private int _selectedSubPage = 0;
    
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty] private string _searchText = string.Empty;
    public ObservableDictionary<Guid, Person> Persons { get; } = [];

    public HistoryPageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
        Persons.AddRange(ProfileConfigHandler.Data.Profile.Persons);
    }
}