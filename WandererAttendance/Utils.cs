using System;
using System.IO;

namespace WandererAttendance;

public static partial class Utils
{
    public static string GetFilePath(params string[] strings)
    {
        string basePath;
        if (OperatingSystem.IsBrowser())
        {
            basePath = "";
        }
        else if (OperatingSystem.IsAndroid())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "WandererAttendance";
            basePath = Path.Combine([appData, appName]);
        }
        else
        {
            basePath = Path.Combine([AppContext.BaseDirectory, "data"]);
        }
        
        var path = Path.Combine([basePath, ..strings]);

        if (!OperatingSystem.IsBrowser())
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        return path;
    }
}