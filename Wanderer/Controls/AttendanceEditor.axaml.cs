using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Wanderer.Abstraction;
using Wanderer.Models;
using Wanderer.Services;
using Wanderer.Services.Config;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Controls;

public partial class AttendanceEditor : UserControl
{
    public partial class AttendanceEditorModel : ObservableRecipient, IDisposable
    {
        public ProfileService ProfileService { get; } = IAppHost.GetService<ProfileService>();
        public MainConfigHandler MainConfigHandler { get; } = IAppHost.GetService<MainConfigHandler>();

        private readonly IDisposable _cleanUp;
        private OneDayAttendanceStatus _attendanceStatus = new();
        private readonly SourceList<KeyValuePair<Guid, Person>> _personSource = new();
        private DateOnly _lastDate = new();

        private readonly ReadOnlyObservableCollection<PersonWithStatus> _persons;
        public ReadOnlyObservableCollection<PersonWithStatus> Persons => _persons;

        public AttendanceEditorModel()
        {
            _cleanUp = _personSource.Connect()
                .Transform(kvp => new PersonWithStatus(kvp.Key, kvp.Value, _attendanceStatus))
                .DisposeMany()
                .Bind(out _persons)
                .Subscribe();
        }
        
        public void UpdateDate(DateOnly date)
        {
            // 检查是否一致。
            if (_lastDate != date && _lastDate != new DateOnly())
            {
                CheckIsChanged(_lastDate);
            }

            _lastDate = date;
            _attendanceStatus = ProfileService.ProfileConfigHandler.Data.Statuses.GetValueOrDefault(date, new OneDayAttendanceStatus());
            
            // hard reload
            var cache = _personSource.Items.Select(i => i).ToList();
            _personSource.Clear();
            _personSource.AddRange(cache);
        }
        
        public void UpdatePersons(IDictionary<Guid, Person> persons)
        {
            _personSource.Clear();
            _personSource.AddRange(persons);
        }

        private void CheckIsChanged(DateOnly date)
        {
            var beforeStatus = ProfileService.ProfileConfigHandler.Data.Statuses.GetValueOrDefault(_lastDate, new OneDayAttendanceStatus());
            var beforeJson = JsonSerializer.Serialize(beforeStatus);
            var afterJson = JsonSerializer.Serialize(_attendanceStatus);

            if (beforeJson != afterJson)
            {
                ProfileService.ProfileConfigHandler.Data.Statuses[date] = _attendanceStatus;
            }
        }

        public void Dispose()
        {
            CheckIsChanged(_lastDate);
            
            _cleanUp.Dispose();
            _personSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    
    public static readonly StyledProperty<ObservableDictionary<Guid, Person>?> PersonsProperty =
        AvaloniaProperty.Register<AttendanceEditor, ObservableDictionary<Guid, Person>?>(nameof(Persons), defaultBindingMode: BindingMode.OneWay);

    public ObservableDictionary<Guid, Person>? Persons
    {
        get => GetValue(PersonsProperty);
        set => SetValue(PersonsProperty, value);
    }
    
    public static readonly StyledProperty<DateTime> DateProperty =
        AvaloniaProperty.Register<AttendanceEditor, DateTime>(nameof(Date), DateTime.Today);

    public DateTime Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }
    
    public AttendanceEditorModel Model { get; } = new();

    static AttendanceEditor()
    {
        PersonsProperty.Changed.AddClassHandler<AttendanceEditor>((x, e) => x.OnPersonsChanged(e));
        DateProperty.Changed.AddClassHandler<AttendanceEditor>((x, e) => x.OnDateChanged(e));
    }
    
    public AttendanceEditor()
    {
        InitializeComponent();
        Persons = [];
        Model.UpdateDate(DateOnly.FromDateTime(Date));
        Model.UpdatePersons(Persons ?? []);
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

    private void OnDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DateTime date)
        {
            Model.UpdateDate(DateOnly.FromDateTime(date));
        }
    }

    private void Persons_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        Model.UpdatePersons(Persons ?? []);
    }

    private void Control_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Persons?.CollectionChanged -= Persons_OnCollectionChanged;
        Model.Dispose();
    }
}
