using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Wanderer.Helpers;

[SupportedOSPlatform("browser")]
public static partial class BrowserLocalStorage
{
    [JSImport("globalThis.window.localStorage.setItem")]
    public static partial void SetItem(string key, string value);

    [JSImport("globalThis.window.localStorage.getItem")]
    public static partial string? GetItem(string key);
    
    [JSImport("globalThis.window.localStorage.removeItem")]
    public static partial void RemoveItem(string key);

    [JSImport("globalThis.window.getLocalStorageKeys")]
    public static partial string[] GetKeys();
}