using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia.Android;

namespace Wanderer.Android;

[Activity(
    Label = "Wanderer",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}

// Avalonia 12：App 初始化改由 AvaloniaAndroidApplication<TApp> 负责，
// MainActivity 不再派生自 AvaloniaMainActivity<TApp>，也不再重写 CreateAppBuilder/CustomizeAppBuilder。
[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }
}
