using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Wanderer.Models;
using Wanderer.Services.Config;
using Wanderer.Shared.Enums;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

/// <summary>
///     选人下拉框中的候选项：档案人员表中的键与人本身。
/// </summary>
public sealed class PersonChoice
{
    public Guid Key { get; init; }

    public Person Person { get; init; } = new();

    public string Name => Person.Name;

    public string Id => Person.Id;

    public HumanSex Sex => Person.Sex;
}

/// <summary>
///     座位页中的一个座位。设置模式下由「有人」复选框与选人框驱动，
///     正常模式下显示姓名、编号与考勤控件。
/// </summary>
public partial class SeatItemViewModel : ObservableObject, IDisposable
{
    private readonly SeatPageViewModel _owner;
    private bool _isSyncing;

    public SeatItemViewModel(Seat model, SeatPageViewModel owner)
    {
        Model = model;
        _owner = owner;

        var choice = owner.FindPerson(model.PersonKey);
        if (choice is null)
        {
            // 人员已被删除：座位按空座位处理。
            model.PersonKey = null;
            return;
        }

        _occupant = choice;
        _occupantText = choice.Name;
        _isOccupied = true;
    }

    /// <summary>
    ///     对应的持久化座位。
    /// </summary>
    public Seat Model { get; }

    /// <summary>
    ///     设置模式下的「有人」复选框。
    /// </summary>
    [ObservableProperty]
    private bool _isOccupied;

    /// <summary>
    ///     当前占用者。
    /// </summary>
    [ObservableProperty]
    private PersonChoice? _occupant;

    /// <summary>
    ///     选人框中的文本。
    /// </summary>
    [ObservableProperty]
    private string _occupantText = string.Empty;

    /// <summary>
    ///     选人框中输入了档案里没有的姓名。
    /// </summary>
    [ObservableProperty]
    private bool _isUnmatched;

    /// <summary>
    ///     当前日期的考勤行。设置模式下为 null。
    /// </summary>
    [ObservableProperty]
    private PersonWithStatus? _attendance;

    public string DisplayName => Occupant?.Name ?? string.Empty;

    public string DisplayId => Occupant?.Id ?? string.Empty;

    public bool HasDisplayId => !string.IsNullOrWhiteSpace(DisplayId);

    public bool HasOccupant => Occupant is not null;

    public bool IsEmpty => Occupant is null;

    /// <summary>
    ///     取当前人员在当前日期的考勤行；没有占用者时不建立。
    /// </summary>
    public void RefreshAttendance(DateOnly date)
    {
        ClearAttendance();

        if (Occupant is not { } choice)
        {
            return;
        }

        var handler = _owner.ProfileConfigHandler;
        var stored = handler.Data.Statuses.GetValueOrDefault(date)?.Persons.GetValueOrDefault(choice.Key)
                     ?? ProfileConfigHandler.CreateDefaultStatus(handler.Data.Profile);

        Attendance = new PersonWithStatus(choice.Key, choice.Person, stored, handler,
                                          _ => _owner.OnSeatStatusChanged(this));
    }

    public void ClearAttendance()
    {
        Attendance?.Dispose();
        Attendance = null;
    }

    public void Dispose()
    {
        ClearAttendance();
    }

    partial void OnIsOccupiedChanged(bool value)
    {
        if (_isSyncing)
        {
            return;
        }

        if (!value)
        {
            _isSyncing = true;
            Occupant = null;
            OccupantText = string.Empty;
            IsUnmatched = false;
            _isSyncing = false;
        }

        Commit();
    }

    partial void OnOccupantChanged(PersonChoice? value)
    {
        if (_isSyncing)
        {
            return;
        }

        if (value is null && IsOccupied && _owner.ResolvePerson(OccupantText, this) is { } restored)
        {
            // 选人框的容器被重建/复用时会先把 SelectedItem 写回 null，此时文本框里仍是有效姓名，
            // 直接以文本为准恢复占用者，避免把已经排好的座位清空。
            _isSyncing = true;
            Occupant = restored;
            _isSyncing = false;
            return;
        }

        _isSyncing = true;
        if (value is not null)
        {
            IsOccupied = true;
            if (!string.Equals(OccupantText, value.Name, StringComparison.Ordinal))
            {
                OccupantText = value.Name;
            }

            IsUnmatched = false;
        }

        _isSyncing = false;

        Commit();
    }

    partial void OnOccupantTextChanged(string value)
    {
        if (_isSyncing)
        {
            return;
        }

        var text = value?.Trim() ?? string.Empty;

        _isSyncing = true;
        switch (text.Length)
        {
            case 0:
                Occupant = null;
                IsUnmatched = false;
                break;
            default:
                var resolved = _owner.ResolvePerson(text, this);
                Occupant = resolved;
                IsUnmatched = resolved is null;
                break;
        }

        _isSyncing = false;

        Commit();
    }

    /// <summary>
    ///     把当前指派写入座位，并在发生变化时落盘。
    /// </summary>
    private void Commit()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(DisplayId));
        OnPropertyChanged(nameof(HasDisplayId));
        OnPropertyChanged(nameof(HasOccupant));
        OnPropertyChanged(nameof(IsEmpty));

        // 以占用者为准；占用者被清空但文本仍是有效人员时按文本恢复，不误删座位。
        var key = IsOccupied
                      ? Occupant?.Key ?? _owner.ResolvePerson(OccupantText, this)?.Key
                      : null;

        if (Model.PersonKey == key)
        {
            return;
        }

        Model.PersonKey = key;
        _owner.SaveLayout();
    }
}
