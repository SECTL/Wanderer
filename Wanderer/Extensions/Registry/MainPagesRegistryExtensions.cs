using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wanderer.Attributes;
using Wanderer.Services;

namespace Wanderer.Extensions.Registry;

public static class MainPagesRegistryExtensions
{
    public static IServiceCollection AddMainPage<T>(this IServiceCollection services) where T : UserControl
    {
        return services.AddMainPageTo<T>(MainPagesRegistryService.Items);
    }
    
    public static IServiceCollection AddMainPageSeparator(this IServiceCollection services)
    {
        MainPagesRegistryService.Items.Add(new MainPageInfo(true));
        return services;
    }
    
    public static IServiceCollection AddMainPageFooter<T>(this IServiceCollection services) where T : UserControl
    {
        return services.AddMainPageTo<T>(MainPagesRegistryService.FooterItems);
    }
    
    public static IServiceCollection AddMainPageFooterSeparator(this IServiceCollection services)
    {
        MainPagesRegistryService.FooterItems.Add(new MainPageInfo(true));
        return services;
    }

    private static IServiceCollection AddMainPageTo<T>(this IServiceCollection services, IList<MainPageInfo> list) where T : UserControl
    {
        var type = typeof(T);
        if (type.GetCustomAttributes(false).FirstOrDefault(x => x is MainPageInfo) is not MainPageInfo info)
        {
            throw new ArgumentException($"无法注册设置页面 {type.FullName}，因为设置页面没有注册信息。");
        }

        if (list.FirstOrDefault(x => x.Id == info.Id) != null)
        {
            throw new ArgumentException($"此设置页面id {info.Id} 已经被占用。");
        }
        
        services.AddKeyedTransient<UserControl, T>(info.Id);
        list.Add(info);
        return services;
    }
}