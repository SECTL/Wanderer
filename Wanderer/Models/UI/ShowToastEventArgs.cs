using Avalonia.Interactivity;
using Wanderer.Controls;

namespace Wanderer.Models.UI;

/// <summary>
/// 显示 Toast 事件参数。
/// </summary>
public class ShowToastEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Toast 事件包含的消息。
    /// </summary>
    public ToastMessage Message { get; }

    internal ShowToastEventArgs(ToastMessage message) : base(AppToastAdorner.ShowToastEvent)
    {
        Message = message;
    }
}