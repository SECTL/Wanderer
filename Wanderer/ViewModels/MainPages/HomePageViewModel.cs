using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wanderer.ViewModels.MainPages;

public partial class HomePageViewModel : ObservableRecipient
{
    public DateOnly TodayDate { get; } = DateOnly.FromDateTime(DateTime.Now);
}