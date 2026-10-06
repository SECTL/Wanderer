using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Microsoft.Extensions.Logging;
using Wanderer.Abstraction;
using Wanderer.Extensions;
using Wanderer.Models;
using Wanderer.Services.Config;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Controls;

public partial class AttendanceEditor : UserControl
{
    public static readonly StyledProperty<ObservableDictionary<Guid, Person>?> PersonsProperty =
        AvaloniaProperty.Register<AttendanceEditor, ObservableDictionary<Guid, Person>?>(
            nameof(Persons), defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<DateOnly> DateProperty =
        AvaloniaProperty.Register<AttendanceEditor, DateOnly>(
            nameof(Date), DateOnly.FromDateTime(DateTime.Today));

    public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<AttendanceEditor, string>(nameof(SearchText), string.Empty);

    public AttendanceEditor()
    {
        InitializeComponent();

        Persons = [];
        Model.UpdatePersons(Persons);
    }

    public ObservableDictionary<Guid, Person>? Persons
    {
        get => GetValue(PersonsProperty);
        set => SetValue(PersonsProperty, value);
    }

    /// <summary>
    ///     当前编辑的日期。
    /// </summary>
    public DateOnly Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    /// <summary>
    ///     搜索文本。过滤在模型内部完成，不会重建人员行。
    /// </summary>
    public string SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public AttendanceEditorModel Model { get; } = new();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PersonsProperty)
        {
            OnPersonsChanged(change);
        }
        else if (change.Property == DateProperty)
        {
            Model.UpdateDate(Date);
        }
        else if (change.Property == SearchTextProperty)
        {
            Model.SearchText = SearchText ?? string.Empty;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Persons is { } persons)
        {
            persons.CollectionChanged -= Persons_OnCollectionChanged;
            persons.CollectionChanged += Persons_OnCollectionChanged;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (Persons is { } persons)
        {
            persons.CollectionChanged -= Persons_OnCollectionChanged;
        }

        // 离开页面时把尚未落盘的改动写入档案。模型保持存活，以便控件被重新挂载后继续工作。
        Model.Flush();
    }

    private void OnPersonsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ObservableDictionary<Guid, Person> oldPersons)
        {
            oldPersons.CollectionChanged -= Persons_OnCollectionChanged;
        }

        if (e.NewValue is ObservableDictionary<Guid, Person> newPersons)
        {
            newPersons.CollectionChanged += Persons_OnCollectionChanged;
            Model.UpdatePersons(newPersons);
        }
        else
        {
            Model.UpdatePersons(new Dictionary<Guid, Person>());
        }
    }

    private void Persons_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        Model.UpdatePersons(Persons ?? []);
    }

    /// <summary>
    ///     考勤编辑器模型。负责当天记录的读取、人员行的构建、脏状态跟踪以及落盘。
    /// </summary>
    public class AttendanceEditorModel : ObservableRecipient
    {
        private readonly SourceList<KeyValuePair<Guid, Person>> _personSource = new();
        private readonly ReadOnlyObservableCollection<PersonWithStatus> _rows;
        private readonly IDisposable _cleanUp;

        private OneDayAttendanceStatus _dayStatus = new();
        private DateOnly _date = DateOnly.FromDateTime(DateTime.Today);
        private string _searchText = string.Empty;
        private bool _isDirty;

        public AttendanceEditorModel()
        {
            _cleanUp = _personSource.Connect()
                                    .Filter(kvp => IsMatch(kvp.Value))
                                    .Transform(kvp => CreateRow(kvp.Key, kvp.Value))
                                    .DisposeMany()
                                    .Bind(out _rows)
                                    .Subscribe();

            Logger = IAppHost.TryGetService<ILogger<AttendanceEditorModel>>();
            ReloadDayStatus();
        }

        public ProfileConfigHandler ProfileConfigHandler { get; } = IAppHost.GetService<ProfileConfigHandler>();
        public ILogger<AttendanceEditorModel>? Logger { get; }

        /// <summary>
        ///     当前显示的人员行。
        /// </summary>
        public ReadOnlyObservableCollection<PersonWithStatus> Persons => _rows;

        /// <summary>
        ///     搜索文本。变更时只重新过滤，不重建人员行。
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                RefreshRows();
            }
        }

        /// <summary>
        ///     当天是否存在尚未落盘的改动。
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (value == _isDirty) return;
                _isDirty = value;
                OnPropertyChanged();
                IsDirtyChanged?.Invoke(value);
            }
        }

        /// <summary>
        ///     脏状态发生变化时触发。
        /// </summary>
        public event Action<bool>? IsDirtyChanged;

        /// <summary>
        ///     同步人员列表。人员、日期变化时重建人员行。
        /// </summary>
        public void UpdatePersons(IDictionary<Guid, Person> persons)
        {
            _personSource.Clear();
            _personSource.AddRange(persons);
        }

        /// <summary>
        ///     切换到某一天，并冲刷前一天尚未落盘的改动。
        /// </summary>
        public void UpdateDate(DateOnly date)
        {
            if (_date == date) return;

            Flush();
            _date = date;
            ReloadDayStatus();
            RefreshRows();
        }

        /// <summary>
        ///     把当天尚未落盘的改动写入档案。
        /// </summary>
        public void Flush()
        {
            if (!IsDirty) return;

            ProfileConfigHandler.Data.Statuses[_date] = _dayStatus;
            Logger?.LogDebug("已保存 {DATE} 的考勤记录。", _date);
            IsDirty = false;
        }

        /// <summary>
        ///     读取当天的考勤记录。当天没有记录时仅创建内存记录，不做写入。
        /// </summary>
        private void ReloadDayStatus()
        {
            _dayStatus = ProfileConfigHandler.Data.Statuses.GetValueOrDefault(_date)
                         ?? new OneDayAttendanceStatus();

            Logger?.LogDebug("已载入 {DATE} 的考勤记录，共 {COUNT} 条。", _date, _dayStatus.Persons.Count);
        }

        private PersonWithStatus CreateRow(Guid personId, Person person)
        {
            if (!_dayStatus.Persons.TryGetValue(personId, out var stored) || stored is null)
            {
                // 当天记录中还没有该人员的条目时，以默认状态建立一个条目。
                stored = ProfileConfigHandler.CreateDefaultStatus(ProfileConfigHandler.Data.Profile);
                _dayStatus.Persons[personId] = stored;
            }

            return new PersonWithStatus(personId, person, stored, ProfileConfigHandler, OnRowChanged);
        }

        /// <summary>
        ///     重建人员行，让它们指向当前日期的记录。
        /// </summary>
        private void RefreshRows()
        {
            var persons = _personSource.Items.ToList();
            _personSource.Clear();
            _personSource.AddRange(persons);
        }

        private void OnRowChanged(Guid personId)
        {
            // 以当天记录的内容为准，不受当前搜索过滤影响。
            IsDirty = _dayStatus.Persons.Values.Any(
                status => ProfileConfigHandler.IsDifferentFromDefault(status,
                                                                     ProfileConfigHandler.Data.Profile));
        }

        private bool IsMatch(Person person)
        {
            return string.IsNullOrWhiteSpace(_searchText) || person.IsMatch(_searchText);
        }
    }
}
