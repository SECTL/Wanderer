using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wanderer.Abstraction;

public abstract class ConfigBase : ObservableRecipient
{
    [JsonIgnore]
    public abstract string ConfigFilePath { get; }
}