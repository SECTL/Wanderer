using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;

namespace Wanderer.Controls;

public class EmptySplashScreen : IApplicationSplashScreen
{
    public async Task RunTasks(CancellationToken cancellationToken) { }

    public string AppName { get; } = "Wanderer";
    public IImage AppIcon { get; } =
        new Bitmap(AssetLoader.Open(new Uri("avares://Wanderer/Assets/AppLogo.png")));
    public object? SplashScreenContent { get; } = null;
    public int MinimumShowTime { get; } = 1000;
}