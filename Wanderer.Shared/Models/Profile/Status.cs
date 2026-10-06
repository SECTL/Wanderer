using CommunityToolkit.Mvvm.ComponentModel;

namespace Wanderer.Shared.Models.Profile;

public partial class Status : ObservableRecipient
{
    [ObservableProperty]
    private bool _isDefault;

    [ObservableProperty]
    private string _name = string.Empty;

    public Status() { }

    public Status(string name, bool isDefault = false)
    {
        Name = name;
        IsDefault = isDefault;
    }
}