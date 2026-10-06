using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wanderer.Abstraction;
using Wanderer.Extensions;
using Wanderer.Models;
using Wanderer.Shared;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Services.Config;

public class ProfileConfigHandler(ILogger<ProfileConfigHandler> logger, ConfigServiceBase configService)
    : ConfigHandlerBase<ProfileConfigModel>(logger, configService, () =>
    {
        var model = new ProfileConfigModel(ProfileService.ProfileName);
        model.Profile.Statuses.AddRange(GlobalConstants.DefaultStatuses);
        return model;
    })
{
    /// <summary>
    ///     启动拼音缓存任务
    /// </summary>
    public void StartPinyinCacheTask()
    {
        Task.Run(() =>
        {
            foreach (var person in Data.Profile.Persons)
            {
                PinyinHelper.GetFullPinyinList(person.Value.Name);
                PinyinHelper.GetFirstPinyinList(person.Value.Name);
            }
        });
    }

    /// <summary>
    ///     构造带有档案中全部默认状态的考勤状态，用于尚未录入考勤的人员。
    /// </summary>
    public static AttendanceStatus CreateDefaultStatus(Profile profile)
    {
        var status = new AttendanceStatus();
        foreach (var kvp in profile.Statuses)
        {
            if (kvp.Value.IsDefault)
            {
                status.Statuses.Add(kvp.Key);
            }
        }

        return status;
    }

    /// <summary>
    ///     判断某个考勤状态是否与档案中定义的默认状态不同，即是否需要写入当天的考勤记录。
    /// </summary>
    public static bool IsDifferentFromDefault(AttendanceStatus status, Profile profile)
    {
        var defaultStatus = CreateDefaultStatus(profile);

        if (status.Statuses.Count != defaultStatus.Statuses.Count)
        {
            return true;
        }

        return status.Statuses.Any(item => !defaultStatus.Statuses.Contains(item));
    }
}
