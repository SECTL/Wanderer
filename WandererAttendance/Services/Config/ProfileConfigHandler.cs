using Microsoft.Extensions.Logging;
using WandererAttendance.Abstraction;
using WandererAttendance.Extensions;
using WandererAttendance.Models;

namespace WandererAttendance.Services.Config;

public class ProfileConfigHandler(ILogger<ProfileConfigHandler> logger, ConfigServiceBase configService)
    : ConfigHandlerBase<ProfileConfigModel>(logger, configService, () =>
    {
        var model = new ProfileConfigModel(ProfileService.ProfileName);
        model.Profile.Statuses.AddRange(GlobalConstants.DefaultStatuses);
        return model;
    });
