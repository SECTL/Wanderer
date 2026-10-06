using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Services.Config;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models;

/// <summary>
///     考勤编辑器中的一个状态芯片。选中状态由自己持有，直接由用户点击驱动，
///     不依赖列表控件的选中机制，因此鼠标与触屏行为一致。
/// </summary>
public partial class StatusChip : ObservableObject
{
    private readonly PersonWithStatus _owner;

    public StatusChip(PersonWithStatus owner, Guid statusId, string name)
    {
        _owner = owner;
        StatusId = statusId;
        Name = name;
        _isChecked = owner.Statuses.Contains(statusId);
    }

    public Guid StatusId { get; }

    public string Name { get; }

    [ObservableProperty]
    private bool _isChecked;

    partial void OnIsCheckedChanged(bool value)
    {
        _owner.ToggleStatus(StatusId, value);
    }
}

/// <summary>
///     考勤编辑器中的人员行。行持有自己的一份状态集合，变更时同步到当天考勤记录。
/// </summary>
public partial class PersonWithStatus : ObservableObject, IDisposable
{
    private bool _isLoaded;
    private bool _isDisposed;

    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private Person _person;

    [ObservableProperty]
    private ObservableCollection<Guid> _statuses = [];

    /// <summary>
    ///     该行当前对应的当天考勤记录条目。
    /// </summary>
    [ObservableProperty]
    private AttendanceStatus _storedStatus;

    public PersonWithStatus(Guid guid, Person person, AttendanceStatus storedStatus,
                            ProfileConfigHandler profileConfigHandler, Action<Guid>? changed = null)
    {
        Guid = guid;
        Person = person;
        _storedStatus = storedStatus;
        ProfileConfigHandler = profileConfigHandler;
        Changed = changed;

        // 用当天记录的内容初始化行，此时不触发同步。
        foreach (var status in storedStatus.Statuses)
        {
            _statuses.Add(status);
        }

        foreach (var kvp in profileConfigHandler.Data.Profile.Statuses)
        {
            StatusChips.Add(new StatusChip(this, kvp.Key, kvp.Value.Name));
        }

        _isLoaded = true;
        Statuses.CollectionChanged += StatusesOnCollectionChanged;
    }

    public ProfileConfigHandler ProfileConfigHandler { get; }

    /// <summary>
    ///     该行的状态芯片。
    /// </summary>
    public ObservableCollection<StatusChip> StatusChips { get; } = [];

    /// <summary>
    ///     该行的状态发生变化时调用，参数为对应的人员 Id。
    /// </summary>
    public Action<Guid>? Changed { get; }

    /// <summary>
    ///     该行的状态是否与档案中的默认状态不同。
    /// </summary>
    public bool IsDifferentFromDefault =>
        ProfileConfigHandler.IsDifferentFromDefault(StoredStatus, ProfileConfigHandler.Data.Profile);

    /// <summary>
    ///     切换某个状态的选中状态，并同步到当天考勤记录。
    /// </summary>
    public void ToggleStatus(Guid statusId, bool isSelected)
    {
        if (isSelected)
        {
            if (!Statuses.Contains(statusId))
            {
                Statuses.Add(statusId);
            }

            return;
        }

        if (Statuses.Contains(statusId))
        {
            Statuses.Remove(statusId);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        Statuses.CollectionChanged -= StatusesOnCollectionChanged;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     把行上的状态同步到当天考勤记录。
    /// </summary>
    private void SyncToStoredStatus()
    {
        StoredStatus.Statuses.Clear();
        foreach (var status in Statuses)
        {
            StoredStatus.Statuses.Add(status);
        }
    }

    private void StatusesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 把行的选中状态回写到芯片，保证撤销等场景下界面同步。
        foreach (var chip in StatusChips)
        {
            var isChecked = Statuses.Contains(chip.StatusId);
            if (chip.IsChecked != isChecked)
            {
                chip.IsChecked = isChecked;
            }
        }

        if (!_isLoaded) return;

        SyncToStoredStatus();
        Changed?.Invoke(Guid);
    }
}
