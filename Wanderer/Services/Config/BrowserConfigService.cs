using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Wanderer.Abstraction;
using Wanderer.Helpers;

namespace Wanderer.Services.Config;

[SupportedOSPlatform("browser")]
public class BrowserConfigService(ILogger<BrowserConfigService> logger) : ConfigServiceBase
{
    private ILogger<BrowserConfigService> Logger { get; } = logger;

    public override bool IsConfigExists<T>(T fallback)
    {
        var filePath = fallback.ConfigFilePath;
        Logger.LogInformation("在 {PATH} 判断配置...", filePath);

        return BrowserLocalStorage.GetItem(filePath) != null;
    }

    public override T LoadConfig<T>(T fallback)
    {
        var filePath = fallback.ConfigFilePath;
        Logger.LogInformation("从 {PATH} 加载配置...", filePath);
        
        var json = BrowserLocalStorage.GetItem(filePath);
        if (json == null)
        {
            Logger.LogWarning("加载失败，正在回滚并保存...");
            SaveConfig(fallback);
            return fallback;
        }
        
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? fallback;
        }
        catch
        {
            Logger.LogWarning("加载失败，正在回滚并保存...");
            SaveConfig(fallback);
            return fallback;
        }
    }

    public override void SaveConfig<T>(T config)
    {
        var filePath = config.ConfigFilePath;
        Logger.LogInformation("往 {PATH} 保存配置...", filePath);
        
        var json = JsonSerializer.Serialize(config, JsonOptions);
        BrowserLocalStorage.SetItem(filePath, json);
    }

    public override void DeleteConfig<T>(T config)
    {
        var filePath = config.ConfigFilePath;
        Logger.LogInformation("在 {PATH} 删除配置...", filePath);
        
        if (BrowserLocalStorage.GetItem(filePath) == null) return;
        BrowserLocalStorage.RemoveItem(filePath);
    }
}