using CommunityToolkit.Mvvm.ComponentModel;

namespace WandererAttendance.Models.Profile;

public partial class Status : ObservableRecipient
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isDefault = false;

    public Status() {}
    
    public Status(string name, bool isDefault = false)
    {
        Name = name;
        IsDefault = isDefault;
    }
}