using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace Wanderer.Behaviors;

public class ListBoxMultiSelectBehavior
{
    public static readonly AttachedProperty<bool> EnableClickToggleProperty =
        AvaloniaProperty.RegisterAttached<ListBoxMultiSelectBehavior, ListBox, bool>("EnableClickToggle");

    static ListBoxMultiSelectBehavior()
    {
        EnableClickToggleProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnEnableClickToggleChanged));
    }

    public static void SetEnableClickToggle(ListBox element, bool value)
        => element.SetValue(EnableClickToggleProperty, value);

    public static bool GetEnableClickToggle(ListBox element)
        => element.GetValue(EnableClickToggleProperty);

    private static void OnEnableClickToggleChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not ListBox listBox) return;
        
        if (e.NewValue.Value)
        {
            listBox.AddHandler(InputElement.PointerPressedEvent, OnListBoxPointerPressed, RoutingStrategies.Tunnel);
        }
        else
        {
            listBox.RemoveHandler(InputElement.PointerPressedEvent, OnListBoxPointerPressed);
        }
    }

    private static void OnListBoxPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox || listBox.SelectionMode == SelectionMode.Single) return;
        
        var clickedItem = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>();
        if (clickedItem == null) return;
        
        clickedItem.IsSelected = !clickedItem.IsSelected;
        e.Handled = true;
    }
}