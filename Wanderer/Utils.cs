using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Wanderer;

public static class Utils
{
    public static string GetDataDirectory()
    {
        if (OperatingSystem.IsBrowser())
        {
            return "";
        }

        if (OperatingSystem.IsAndroid())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = Assembly.GetExecutingAssembly().GetName().Name ?? "Wanderer";
            return Path.Combine([appData, appName]);
        }

        #if DEBUG
        return Path.Combine([AppContext.BaseDirectory, "data"]);
        #else
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appNameDirectory = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Wanderer";
        return Path.Combine([appDataDirectory, appNameDirectory]);
        #endif
    }

    public static string GetFilePath(params string[] strings)
    {
        var basePath = GetDataDirectory();

        var path = Path.Combine([basePath, .. strings]);

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

    public static T CopyObjectByJson<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}