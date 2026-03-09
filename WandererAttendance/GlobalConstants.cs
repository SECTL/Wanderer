using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Media;
using ClassIsland;
using WandererAttendance.Models.Profile;

namespace WandererAttendance;

public static class GlobalConstants
{
    public static FontFamily FluentIconsFontFamily =
        new("avares://WandererAttendance/Assets/Fonts/#FluentSystemIcons-Resizable");

    public static string Tag => GitInfo.Tag;
    public static string Branch => GitInfo.Branch;
    public static string CommitHash => GitInfo.CommitHash[..7];
    public static string FullCommitHash => GitInfo.CommitHash;
    
    public static string CodeName => "Paimon";
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    public static string DisplayVersion => $"{Version} (Codename {CodeName})";
    public static string VersionLong => $"{Version}-{CodeName}-{CommitHash}({Branch})";
    
#if DEBUG
    public static bool IsDevelopment => true;
#else
    public static bool IsDevelopment => false;
#endif
    
    public static class ImportSheetStaticTexts
    {
        public static readonly List<string> NameHeaderTexts = ["姓名", "名字", "name"];
        public static readonly List<string> IdHeaderTexts = ["编号", "学号", "考号", "id"];
        public static readonly List<string> SexHeaderTexts = ["性别", "sex"];

        public static class SexTexts
        {
            public static readonly List<string> Male = ["男", "male", "boy", "man", "1"];
            public static readonly List<string> Female = ["女", "female", "girl", "woman", "0"];
        }
    }
    
    public static readonly Dictionary<Guid, Status> DefaultStatuses = new()
    {
        [Guid.Parse("38D9AD05-360D-44F1-96A6-F1CB141728A3")] = new Status("已到", true),
        [Guid.Parse("51808656-31E6-4F8E-943D-F5A7E305747B")] = new Status("迟到"),
        [Guid.Parse("628F5981-CAA5-443A-9F12-390EFE315E3E")] = new Status("请假"),
    };
}