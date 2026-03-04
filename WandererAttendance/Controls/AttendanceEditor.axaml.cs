using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Abstraction;
using WandererAttendance.Models.Profile;
using WandererAttendance.Services;
using WandererAttendance.Services.Config;

namespace WandererAttendance.Controls;

public partial class AttendanceEditor : UserControl
{
    public partial class PersonWithStatus : ObservableObject, IDisposable
    {
        private readonly Guid _personGuid;
        private readonly OneDayAttendanceStatus _status;
        private readonly IList<Status> _allStatuses;
        private bool _isUpdatingFromService = false;

        public Person Person { get; }
        public ObservableCollection<Status> Statuses { get; } = [];

        public PersonWithStatus(Person person, IList<Status> allStatuses, OneDayAttendanceStatus status)
        {
            Person = person;
            
            _personGuid = person.Guid;
            _allStatuses = allStatuses;
            _status = status;

            LoadStatuses();

            Statuses.CollectionChanged += OnStatusesCollectionChanged;
            _status.Students.CollectionChanged += OnStudentsDictionaryChanged;
        }

        private void LoadStatuses()
        {
            if (_isUpdatingFromService) return; // 防止递归
            _isUpdatingFromService = true;
            
            try
            {
                var statusEntry = _status.Students.GetValueOrDefault(_personGuid);

                Statuses.Clear();
                if (statusEntry == null)
                {
                    statusEntry = new AttendanceStatus();
                    _status.Students[_personGuid] = statusEntry;
                    
                    foreach (var s in _allStatuses.Where(s => s.IsDefault))
                    {
                        Statuses.Add(s);
                        statusEntry.Statuses.Add(s.Guid);
                    }
                }
                else
                {
                    var statusGuids = statusEntry.Statuses;
                    foreach (var guid in statusGuids)
                    {
                        var status = _allStatuses.FirstOrDefault(st => st.Guid == guid);
                        Statuses.Add(status ?? new Status(guid, "???"));
                    }
                }
            }
            finally
            {
                _isUpdatingFromService = false;
            }
        }

        private void OnStudentsDictionaryChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LoadStatuses();
        }

        private void OnStatusesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingFromService)
                return;

            Dispatcher.UIThread.Post(UpdateServiceStatuses);
        }

        private void UpdateServiceStatuses()
        {
            var statusEntry = _status.Students.GetValueOrDefault(_personGuid);
            if (statusEntry == null)
            {
                statusEntry = new AttendanceStatus();
                _status.Students[_personGuid] = statusEntry;
            }

            statusEntry.Statuses.Clear();
            statusEntry.Statuses.AddRange(Statuses.Select(s => s.Guid));
        }

        public void Dispose()
        {
            Statuses.CollectionChanged -= OnStatusesCollectionChanged;
            _status.Students.CollectionChanged -= OnStudentsDictionaryChanged;
            GC.SuppressFinalize(this);
        }
    }
    
    public partial class AttendanceEditorModel : ObservableRecipient, IDisposable
    {
        public ProfileService ProfileService { get; } = IAppHost.GetService<ProfileService>();
        public MainConfigHandler MainConfigHandler { get; } = IAppHost.GetService<MainConfigHandler>();

        private readonly IDisposable _cleanUp;
        private OneDayAttendanceStatus _attendanceStatus = new();
        private readonly SourceList<Person> _personSource = new();

        private readonly ReadOnlyObservableCollection<PersonWithStatus> _persons;
        public ReadOnlyObservableCollection<PersonWithStatus> Persons => _persons;

        public AttendanceEditorModel()
        {
            _cleanUp = _personSource.Connect()
                .Transform(person => new PersonWithStatus(person, ProfileService.ProfileConfigHandler.Data.Profile.Statuses, _attendanceStatus))
                .DisposeMany()
                .Bind(out _persons)
                .Subscribe();
        }
        
        public void UpdateDate(DateOnly date)
        {
            _attendanceStatus = ProfileService.ProfileConfigHandler.Data.Statuses
                .GetValueOrDefault(date, new OneDayAttendanceStatus());
            
            // hard reload
            var cache = _personSource.Items.Select(i => i);
            _personSource.Clear();
            _personSource.AddRange(cache);
        }
        
        public void UpdatePersons(IEnumerable<Person> persons)
        {
            _personSource.Clear();
            _personSource.AddRange(persons);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
            _personSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    
    public static readonly StyledProperty<ObservableCollection<Person>?> PersonsProperty =
        AvaloniaProperty.Register<AttendanceEditor, ObservableCollection<Person>?>(nameof(Persons), defaultBindingMode: BindingMode.OneWay);

    public ObservableCollection<Person>? Persons
    {
        get => GetValue(PersonsProperty);
        set => SetValue(PersonsProperty, value);
    }
    
    public static readonly StyledProperty<DateOnly> DateProperty =
        AvaloniaProperty.Register<AttendanceEditor, DateOnly>(nameof(Date), DateOnly.FromDateTime(DateTime.Now));

    public DateOnly Date
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
        Model.UpdateDate(Date);
        Model.UpdatePersons(Persons ?? []);
    }

    private void OnPersonsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ObservableCollection<Person> oldPersons)
        {
            oldPersons.CollectionChanged -= Persons_OnCollectionChanged;
        }

        if (e.NewValue is ObservableCollection<Person> newPersons)
        {
            newPersons.CollectionChanged += Persons_OnCollectionChanged;
            Model.UpdatePersons(newPersons);
        }
        else
        {
            Model.UpdatePersons([]);
        }
    }

    private void OnDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DateOnly date)
        {
            Model.UpdateDate(date);
        }
    }

    private void Persons_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        Model.UpdatePersons(Persons ?? []);
    }

    private void Control_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Persons?.CollectionChanged -= Persons_OnCollectionChanged;
    }
}
