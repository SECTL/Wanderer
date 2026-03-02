using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Models.Profile;
using WandererAttendance.Services;
using WandererAttendance.Services.Config;

namespace WandererAttendance.ViewModels.MainPages;

public partial class AttendancePageViewModel : ObservableRecipient, IDisposable
{
    public partial class PersonWithStatus : ObservableObject, IDisposable
    {
        private readonly ProfileService _profileService;
        private readonly Guid _personGuid;
        private bool _isUpdatingFromService = false;

        [ObservableProperty]
        private Person _person;

        public ObservableCollection<Status> Statuses { get; } = [];

        public PersonWithStatus(Person person, ProfileService profileService)
        {
            _person = person;
            _personGuid = person.Guid;
            _profileService = profileService;

            LoadStatusesFromService();

            Statuses.CollectionChanged += OnStatusesCollectionChanged;
            _profileService.AttendanceStatus.Students.CollectionChanged += OnStudentsDictionaryChanged;
        }

        private void LoadStatusesFromService()
        {
            if (_isUpdatingFromService) return; // 防止递归
            _isUpdatingFromService = true;
            
            try
            {
                var statusEntry = _profileService.AttendanceStatus.Students.GetValueOrDefault(_personGuid);
                var allStatuses = _profileService.ProfileConfigHandler.Data.Profile.Statuses;

                Statuses.Clear();
                if (statusEntry == null)
                {
                    statusEntry = new AttendanceStatus();
                    _profileService.AttendanceStatus.Students[_personGuid] = statusEntry;
                    
                    foreach (var s in allStatuses.Where(s => s.IsDefault))
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
                        var status = allStatuses.FirstOrDefault(st => st.Guid == guid);
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
            LoadStatusesFromService();
        }

        private void OnStatusesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingFromService)
                return;

            Dispatcher.UIThread.Post(UpdateServiceStatuses);
        }

        private void UpdateServiceStatuses()
        {
            var statusEntry = _profileService.AttendanceStatus.Students.GetValueOrDefault(_personGuid);
            if (statusEntry == null)
            {
                statusEntry = new AttendanceStatus();
                _profileService.AttendanceStatus.Students[_personGuid] = statusEntry;
            }

            statusEntry.Statuses.Clear();
            statusEntry.Statuses.AddRange(Statuses.Select(s => s.Guid));
        }

        public void Dispose()
        {
            Statuses.CollectionChanged -= OnStatusesCollectionChanged;
            _profileService.AttendanceStatus.Students.CollectionChanged -= OnStudentsDictionaryChanged;
            GC.SuppressFinalize(this);
        }
    }
    
    public ProfileConfigHandler ProfileConfigHandler { get; }
    public ProfileService ProfileService { get; }
    
    private readonly IDisposable _cleanUp;
    public readonly SourceList<Person> PersonSource = new();
    private ReadOnlyObservableCollection<PersonWithStatus> _persons;
    public ReadOnlyObservableCollection<PersonWithStatus> Persons => _persons;

    public DateOnly TodayDate { get; } = DateOnly.FromDateTime(DateTime.Now);
    [ObservableProperty] private string _searchText = string.Empty;
    
    public AttendancePageViewModel(ProfileService profileService, ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
        ProfileService = profileService;

        PersonSource.AddRange(ProfileConfigHandler.Data.Profile.Persons);
        
        _cleanUp = PersonSource.Connect()
            .Transform(person => new PersonWithStatus(person, ProfileService))
            .DisposeMany()
            .Bind(out _persons)
            .Subscribe();
    }
    
    public void Dispose()
    {
        _cleanUp.Dispose();
        PersonSource.Dispose();
        GC.SuppressFinalize(this);
    }
}