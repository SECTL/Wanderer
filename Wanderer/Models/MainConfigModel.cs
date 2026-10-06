using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Abstraction;
using Wanderer.Enums;

namespace Wanderer.Models;

public partial class MainConfigModel : ConfigBase
{
    [ObservableProperty]
    private BackgroundEffect _backgroundEffect = BackgroundEffect.Mica;

    [ObservableProperty]
    private string _profileName = "Default";

    [ObservableProperty]
    private StatusChangerShowMode _statusChangerShowMode = StatusChangerShowMode.ChipListBox;

    [JsonIgnore]
    public override string ConfigFilePath => Utils.GetFilePath("Config.json");
}