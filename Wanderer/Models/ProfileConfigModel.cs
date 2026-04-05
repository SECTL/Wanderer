using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Abstraction;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models;

public partial class ProfileConfigModel : ConfigBase, IProfileModel
{
    [JsonIgnore]
    public override string ConfigFilePath => Utils.GetFilePath("Profiles", $"{Profile.Name}.json");
    
    [ObservableProperty] private Profile _profile;
    [ObservableProperty] private ObservableDictionary<DateOnly, OneDayAttendanceStatus> _statuses = [];

    public ProfileConfigModel()
    {
        Profile = new Profile();
    }
    
    public ProfileConfigModel(string name)
    {
        Profile = new Profile { Name = name };
    }
}