using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WandererAttendance.ComponentModels;

namespace WandererAttendance.Models.Profile;

public partial class Profile : ObservableRecipient
{
    public string Name { get; set; } = "EMPTY";
    [ObservableProperty] private ObservableDictionary<Guid, Person> _persons = [];
    [ObservableProperty] private ObservableDictionary<Guid, Status> _statuses = [];
    [ObservableProperty] private ObservableDictionary<Guid, Tag> _tags = [];
}