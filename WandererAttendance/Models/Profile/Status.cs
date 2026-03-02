using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WandererAttendance.Models.Profile;

public partial class Status : ObservableRecipient
{
    [ObservableProperty] private Guid _guid = Guid.NewGuid();
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isDefault = false;

    public Status() {}
    
    public Status(string name, bool isDefault = false)
    {
        Name = name;
        IsDefault = isDefault;
    }
    
    public Status(Guid guid, string name, bool isDefault = false)
    {
        Guid = guid;
        Name = name;
        IsDefault = isDefault;
    }
}