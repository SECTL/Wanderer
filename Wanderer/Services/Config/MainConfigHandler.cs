using Microsoft.Extensions.Logging;
using Wanderer.Abstraction;
using Wanderer.Models;

namespace Wanderer.Services.Config;

public class MainConfigHandler(ILogger<MainConfigHandler> logger, ConfigServiceBase configService)
    : ConfigHandlerBase<MainConfigModel>(logger, configService, () => new MainConfigModel());