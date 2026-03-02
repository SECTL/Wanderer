using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WandererAttendance.Models.Profile;

public partial class Profile : ObservableRecipient
{
    public string Name { get; set; } = "EMPTY";
    [ObservableProperty] private ObservableCollection<Person> _persons = [];
    [ObservableProperty] private ObservableCollection<Status> _statuses = [];
}