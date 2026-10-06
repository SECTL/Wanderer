using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Wanderer.Abstraction;
using Wanderer.Services;

namespace Wanderer.Converters;

public class GuidToStatusNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Guid guid)
        {
            return "???";
        }

        var service = IAppHost.GetService<ProfileService>();
        return service.ProfileConfigHandler.Data.Profile.Statuses.TryGetValue(guid, out var status)
                   ? status.Name
                   : "未知状态";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}