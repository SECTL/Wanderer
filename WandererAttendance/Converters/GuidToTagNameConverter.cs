using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using WandererAttendance.Abstraction;
using WandererAttendance.Services;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Converters;

public class GuidToTagNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Guid guid)
        {
            return "???";
        }

        var service = IAppHost.GetService<ProfileService>();
        return service.ProfileConfigHandler.Data.Profile.Tags
            .FirstOrDefault(kvp => kvp.Key == guid, KeyValuePair.Create(guid, new Tag("???")))
            .Value.Name;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}