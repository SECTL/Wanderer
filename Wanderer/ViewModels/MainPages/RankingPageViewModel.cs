using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Wanderer.Extensions;
using Wanderer.Models;
using Wanderer.Models.Ranking;
using Wanderer.Services.Config;
using Wanderer.Shared.ComponentModels;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.ViewModels.MainPages;

public partial class RankingPageViewModel : ObservableRecipient
{
    // 按人员查看
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _selectedPage;

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
                                         .Select(s => BuildStatusRanking(configData, s.Value, s.Key)));

        // "无状态"：当天有考勤记录、但该人员一个状态都没有
        StatusRanking.Add(BuildStatusRanking(configData, StatusAndCount.NoStatus, null));

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

        DataGridColumns.Add(new DataGridTextColumn
        {
            Header = StatusAndCount.NoStatusTitle,
            IsReadOnly = true,
            CustomSortComparer = new NoStatusCountComparer(),
            Binding = new Binding(nameof(PersonWithStatusCounts.NoStatusCount))
        });

        UpdatePersonWithStatusCountsList();
    }

    public ProfileConfigHandler ProfileConfigHandler { get; }

    // 按状态查看
    public ObservableCollection<StatusWithRanking> StatusRanking { get; } = [];
    public ObservableDictionary<Guid, Person> Persons { get; } = [];

    public ObservableCollection<DataGridColumn> DataGridColumns { get; } = [];
    public ObservableCollection<PersonWithStatusCounts> PersonWithStatusCountsList { get; } = [];

    public void UpdatePersonWithStatusCountsList()
    {
        var configData = ProfileConfigHandler.Data;
        var defaultStatus = ProfileConfigHandler.CreateDefaultStatus(configData.Profile);

        PersonWithStatusCountsList.Clear();
        PersonWithStatusCountsList.AddRange(
            Persons.Select(kvp =>
            {
                Dictionary<Guid, int> counts = [];
                var noStatusCount = 0;
                foreach (var oneDayAttendanceStatus in configData.Statuses.Values)
                {
                    var attendanceStatus = oneDayAttendanceStatus.Persons.GetValueOrDefault(kvp.Key)
                                           ?? defaultStatus;
                    if (attendanceStatus.Statuses.Count == 0)
                    {
                        // 当天有记录、但该人员一个状态都没有
                        noStatusCount++;
                        continue;
                    }

                    foreach (var status in attendanceStatus.Statuses)
                    {
                        counts[status] = counts.GetValueOrDefault(status, 0) + 1;
                    }
                }

                return new PersonWithStatusCounts
                {
                    Person = kvp.Value,
                    StatusCounts = configData.Profile.Statuses.Keys
                                             .Select(status => counts.GetValueOrDefault(status, 0))
                                             .ToList(),
                    NoStatusCount = noStatusCount
                };
            }));
    }

    /// <summary>
    ///     统计某个状态在各天的上榜情况。<paramref name="statusId" /> 为 <see langword="null" /> 时统计"无状态"。
    /// </summary>
    private static StatusWithRanking BuildStatusRanking(ProfileConfigModel configData, Status status, Guid? statusId)
    {
        var defaultStatus = ProfileConfigHandler.CreateDefaultStatus(configData.Profile);
        Dictionary<Guid, int> counts = [];
        foreach (var person in configData.Profile.Persons.Keys)
        {
            foreach (var oneDayAttendanceStatus in configData.Statuses.Values)
            {
                // 当天记录中缺少该人员时按默认状态处理。
                var attendanceStatus = oneDayAttendanceStatus.Persons.GetValueOrDefault(person) ?? defaultStatus;
                var hit = statusId is { } id
                              ? attendanceStatus.Statuses.Contains(id)
                              : attendanceStatus.Statuses.Count == 0;

                if (hit)
                {
                    counts[person] = counts.GetValueOrDefault(person, 0) + 1;
                }
            }
        }

        var items = counts
                    .Where(kvp => kvp.Value > 0)
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp => new StatusWithRankingItem
                    {
                        Person = configData.Profile.Persons[kvp.Key],
                        Count = kvp.Value
                    })
                    .ToList();

        return new StatusWithRanking
        {
            Status = status,
            Items = items
        };
    }

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

    private class NoStatusCountComparer : IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is PersonWithStatusCounts c1 && y is PersonWithStatusCounts c2)
            {
                return c1.NoStatusCount.CompareTo(c2.NoStatusCount);
            }

            return 0;
        }
    }
}