using System.Threading.Tasks;
using Avalonia;
using FluentAvalonia.UI.Controls;

namespace Wanderer.Shared;

public class CommonTaskDialogs
{
    public static async Task<object?> ShowDialog(string header, string content, Visual? xamlRoot = null)
    {
        var dialog = new FATaskDialog
        {
            Content = content,
            Header = header,
            Buttons =
            {
                new FATaskDialogButton("确定", true)
                {
                    IsDefault = true
                }
            },
            XamlRoot = xamlRoot ?? App.GetRootWindow()
        };

        return await dialog.ShowAsync();
    }
}