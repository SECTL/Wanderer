using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using WandererAttendance.Abstraction;
using WandererAttendance.Shared.Models.Profile;
using WandererAttendance.Shared.ComponentModels;
using WandererAttendance.Shared.Models;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Models;

public partial class ProfileConfigModel : ConfigBase, IProfileModel
{
    [JsonIgnore]
    public override string ConfigFilePath => Utils.GetFilePath("Profiles", $"{Profile.Name}.json");
    
    [ObservableProperty] private Shared.Models.Profile.Profile _profile;
    [ObservableProperty] private ObservableDictionary<DateOnly, OneDayAttendanceStatus> _statuses = [];

    public ProfileConfigModel()
    {
        Profile = new Shared.Models.Profile.Profile();
    }
    
    public ProfileConfigModel(string name)
    {
        Profile = new Shared.Models.Profile.Profile { Name = name };
    }
}