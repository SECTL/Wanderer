using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Wanderer.Abstraction;
using Wanderer.Models;
using Wanderer.Services.Config;

namespace Wanderer.Controls;

/// <summary>
///     单个人的考勤状态编辑控件。按全局设置 <see cref="MainConfigHandler" /> 的
///     考勤状态修改样式显示状态芯片或多选下拉框，由考勤编辑器与座位页共用。
/// </summary>
public partial class PersonAttendanceEditor : UserControl
{
    public static readonly StyledProperty<PersonWithStatus?> RowProperty =
        AvaloniaProperty.Register<PersonAttendanceEditor, PersonWithStatus?>(nameof(Row));

    public PersonAttendanceEditor()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     要编辑的人员行。为 null 时不显示内容。
    /// </summary>
    public PersonWithStatus? Row
    {
        get => GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    public MainConfigHandler MainConfigHandler { get; } = IAppHost.GetService<MainConfigHandler>();

    /// <summary>
    ///     状态芯片点击。自己翻转选中状态，不依赖列表控件的选中机制，
    ///     因此鼠标、触屏、笔的行为完全一致。
    /// </summary>
    private void StatusChip_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { DataContext: StatusChip chip } button)
        {
            return;
        }

        chip.IsChecked = button.IsChecked == true;
    }
}
