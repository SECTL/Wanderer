using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Rendering;

namespace Wanderer.Helpers;

internal static class AvaloniaUnsafeAccessorHelpers
{
    public enum Win32CompositionMode
    {
        WinUIComposition = 1,
        DirectComposition = 2,
        LowLatencyDxgiSwapChain = 3,
        RedirectionSurface = 4
    }

    private static IAvaloniaDependencyResolver? AvaloniaLocator { get; } = GetCurrentAvaloniaLocator(null);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Current")]
    private static extern IAvaloniaDependencyResolver? GetCurrentAvaloniaLocator(AvaloniaLocator? nullLocator);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetService")]
    private static extern object? GetAvaloniaDependencyService(IAvaloniaDependencyResolver? avaloniaLocator,
        Type serviceType);

    public static T? GetAvaloniaLocatorService<T>() where T : class
    {
        if (AvaloniaLocator is null)
            return null;
        var result = GetAvaloniaDependencyService(AvaloniaLocator, typeof(T));
        return result as T;
    }
    
    public static Win32CompositionMode? GetActiveWin32CompositionMode()
    {
        // Avalonia 12 不再向 AvaloniaLocator 注册 IRenderTimer。
        // 合成连接通过 RenderLoop.FromTimer(connection) 注册为 IRenderLoop，
        // 因此这里取出 IRenderLoop，再从 DefaultRenderLoop 的私有 _timer 字段
        // 还原出真正的合成模式。
        var renderLoop = GetAvaloniaLocatorService<IRenderLoop>();
        if (renderLoop is null)
            return Win32CompositionMode.RedirectionSurface;

        var timerField = renderLoop.GetType().GetField("_timer",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var timer = timerField?.GetValue(renderLoop);
        var timerClassName = timer?.GetType().Name;

        return timerClassName switch
        {
            "WinUiCompositorConnection" => Win32CompositionMode.WinUIComposition,
            "DirectCompositionConnection" => Win32CompositionMode.DirectComposition,
            "DxgiConnection" => Win32CompositionMode.LowLatencyDxgiSwapChain,
            _ => Win32CompositionMode.RedirectionSurface
        };
    }
}
