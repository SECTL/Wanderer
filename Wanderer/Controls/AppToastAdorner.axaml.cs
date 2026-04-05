using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Wanderer.Models.UI;

namespace Wanderer.Controls;

public partial class AppToastAdorner : UserControl
{
    private Control Control { get; }
    public ObservableCollection<ToastMessage> Messages { get; } = [];
    
    public static readonly RoutedEvent<ShowToastEventArgs> ShowToastEvent =
        RoutedEvent.Register<AppToastAdorner, ShowToastEventArgs>(nameof(ShowToast), RoutingStrategies.Bubble);

    // Provide CLR accessors for the event
    public event EventHandler<ShowToastEventArgs> ShowToast
    { 
        add => AddHandler(ShowToastEvent, value);
        remove => RemoveHandler(ShowToastEvent, value);
    }
    
    public AppToastAdorner(Control control)
    {
        Control = control;
        control.AddHandler(ShowToastEvent, OnShowToast);
        control.Unloaded += ControlOnUnloaded;
        InitializeComponent();
    }

    private void ControlOnUnloaded(object? sender, EventArgs e)
    {
        Control.Unloaded -= ControlOnUnloaded;
        Control.RemoveHandler(ShowToastEvent, OnShowToast);
    }

    private void OnShowToast(object? sender, ShowToastEventArgs e)
    {
        Messages.Insert(0, e.Message);
        e.Message.ClosedCancellationTokenSource.Token.Register(() =>
        {
            DispatcherTimer.RunOnce(() => Messages.Remove(e.Message), TimeSpan.FromSeconds(0.3));
        });
        if (e.Message.AutoClose)
        {
            DispatcherTimer.RunOnce(() => e.Message.Close(), e.Message.Duration);
        }
    }

    [RelayCommand]
    private void CloseToast(ToastMessage message)
    {
        message.Close();
    }
}