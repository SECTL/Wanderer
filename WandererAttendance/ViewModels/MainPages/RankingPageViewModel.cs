using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Extensions;
using WandererAttendance.Models.Ranking;
using WandererAttendance.Services.Config;
using WandererAttendance.Shared.ComponentModels;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.ViewModels.MainPages;

public partial class RankingPageViewModel : ObservableRecipient
{
    private class StatusCountComparer(int index) : IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is PersonWithStatusCounts c1 && y is PersonWithStatusCounts c2)
            {
                return c1.StatusCounts[index].CompareTo(c2.StatusCounts[index]);
            }

            return 0;
        }
    }
    
    public ProfileConfigHandler ProfileConfigHandler { get; }
    
    [ObservableProperty] private int _selectedPage = 0;
    
    // 按状态查看
    public ObservableCollection<StatusWithRanking> StatusRanking { get; } = [];
    
    // 按人员查看
    [ObservableProperty] private string _searchText = string.Empty;
    public ObservableDictionary<Guid, Person> Persons { get; } = [];
    
    public ObservableCollection<DataGridColumn> DataGridColumns { get; } = [];
    public ObservableCollection<PersonWithStatusCounts> PersonWithStatusCountsList { get; } = [];

    public RankingPageViewModel(ProfileConfigHandler profileConfigHandler)
    {
        ProfileConfigHandler = profileConfigHandler;
        Persons.AddRange(ProfileConfigHandler.Data.Profile.Persons);
        
        var configData = ProfileConfigHandler.Data;

        var defaultAttendanceStatus = new AttendanceStatus();
        defaultAttendanceStatus.Statuses.AddRange(configData.Profile.Statuses
            .Where(s => s.Value.IsDefault)
            .Select(s => s.Key));
        
        StatusRanking.AddRange(configData.Profile.Statuses
            .Select(s =>
            {
                Dictionary<Guid, int> counts = [];
                foreach (var person in configData.Profile.Persons.Keys)
                {
                    foreach (var oneDayAttendanceStatus in configData.Statuses.Values)
                    {
                        var attendanceStatus =
                            oneDayAttendanceStatus.Persons.GetValueOrDefault(person, defaultAttendanceStatus);
                        
                        if (attendanceStatus.Statuses.Contains(s.Key))
                        {
                            counts[person] = counts.GetValueOrDefault(person, 0) + 1;
                        }
                    }
                }

                List<StatusWithRankingItem> result = [];
                result.AddRange(counts
                    .Select(kvp => new StatusWithRankingItem
                    {
                        Person = configData.Profile.Persons[kvp.Key],
                        Count = kvp.Value
                    })
                    .Where(item => item.Count > 0)
                    .OrderByDescending(item => item.Count));

                return new StatusWithRanking
                {
                    Status = s.Value,
                    Items = result
                };
            }));
        
        foreach (var (index, kvp) in configData.Profile.Statuses.Index())
        {
            DataGridColumns.Add(new DataGridTextColumn
            {
                Header = kvp.Value.Name,
                IsReadOnly = true,
                CustomSortComparer = new StatusCountComparer(index),
                Binding = new Binding($"StatusCounts[{index}]")
            });
        }

        UpdatePersonWithStatusCountsList();
    }

    public void UpdatePersonWithStatusCountsList()
    {
        var configData = ProfileConfigHandler.Data;
        
        var defaultAttendanceStatus = new AttendanceStatus();
        defaultAttendanceStatus.Statuses.AddRange(configData.Profile.Statuses
            .Where(s => s.Value.IsDefault)
            .Select(s => s.Key));
        
        PersonWithStatusCountsList.Clear();
        PersonWithStatusCountsList.AddRange(
            Persons.Select(kvp =>
            {
                Dictionary<Guid, int> counts = [];
                foreach (var status in configData.Profile.Statuses.Keys)
                {
                    counts[status] = 0;

                    foreach (var oneDayAttendanceStatus in configData.Statuses.Values)
                    {
                        var attendanceStatus =
                            oneDayAttendanceStatus.Persons.GetValueOrDefault(kvp.Key, defaultAttendanceStatus);
                        if (attendanceStatus.Statuses.Contains(status))
                        {
                            counts[status]++;
                        }
                    }
                }

                return new PersonWithStatusCounts
                {
                    Person = kvp.Value,
                    StatusCounts = counts.Values.ToList()
                };
            }));
    }
}