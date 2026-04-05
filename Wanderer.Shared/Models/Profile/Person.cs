using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Shared.Enums;

namespace Wanderer.Shared.Models.Profile;

public partial class Person : ObservableRecipient
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private HumanSex _sex = HumanSex.Unknown;
    [ObservableProperty] private ObservableCollection<Guid> _tags = [];
}