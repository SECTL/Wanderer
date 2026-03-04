using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DynamicData;
using Microsoft.Extensions.Logging;
using WandererAttendance.Helpers;
using WandererAttendance.Models.Profile;
using WandererAttendance.Services.Config;

namespace WandererAttendance.Services;

public class ProfileService
{
    public static string ProfileName = "EMPTY";
    public static string ProfilePath => Utils.GetFilePath("Profiles");
    
    public ProfileConfigHandler ProfileConfigHandler { get; }
    private ILogger<ProfileService> Logger { get; }
    public ObservableCollection<string> Profiles { get; } = [];
    public OneDayAttendanceStatus AttendanceStatus { get; }

    public ProfileService(ILogger<ProfileService> logger, ProfileConfigHandler profileConfigHandler)
    {
        Logger = logger;
        ProfileConfigHandler = profileConfigHandler;

        AttendanceStatus = ProfileConfigHandler.Data.Statuses.GetValueOrDefault(
                DateOnly.FromDateTime(DateTime.Now), new OneDayAttendanceStatus());
        
        RefreshProfiles();
    }
    
    public void RefreshProfiles()
    {
        Logger.LogInformation("刷新档案列表");
        Profiles.Clear();

        if (OperatingSystem.IsBrowser())
        {
            Profiles.AddRange(
                from i in BrowserLocalStorage.GetKeys()
                where i.StartsWith(ProfilePath) && i.EndsWith(".json")
                orderby i
                select Path.GetFileName(i).Replace(".json", ""));
        }
        else
        {
            Profiles.AddRange(
                from i in Directory.GetFiles(ProfilePath) 
                where i.EndsWith(".json")
                orderby i
                select Path.GetFileName(i).Replace(".json", ""));
        }
    }
}