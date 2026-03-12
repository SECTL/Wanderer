using CommunityToolkit.Mvvm.ComponentModel;

namespace WandererAttendance.Shared.Models.Profile;

public partial class Tag : ObservableRecipient
{
    [ObservableProperty] private string _name = string.Empty;
    
    public Tag() {}
    
    public Tag(string name)
    {
        Name = name;
    }
}