using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WandererAttendance.Abstraction;
using WandererAttendance.Extensions;
using WandererAttendance.Models;
using WandererAttendance.Shared;

namespace WandererAttendance.Services.Config;

public class ProfileConfigHandler(ILogger<ProfileConfigHandler> logger, ConfigServiceBase configService)
    : ConfigHandlerBase<ProfileConfigModel>(logger, configService, () =>
    {
        var model = new ProfileConfigModel(ProfileService.ProfileName);
        model.Profile.Statuses.AddRange(GlobalConstants.DefaultStatuses);
        return model;
    })
{
    /// <summary>
    /// 启动拼音缓存任务
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
}
