using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using WandererAttendance.Abstraction;
using WandererAttendance.Attributes;
using WandererAttendance.Extensions;
using WandererAttendance.Helpers.UI;
using WandererAttendance.Models;
using WandererAttendance.Models.Ranking;
using WandererAttendance.Shared.Enums;
using WandererAttendance.Shared.Models.Profile;
using WandererAttendance.ViewModels.MainPages;

namespace WandererAttendance.Views.MainPages;

[MainPageInfo("排行榜", "ranking", "\uE3E0", true, true)]
public partial class RankingPage : UserControl
{
    public RankingPageViewModel ViewModel { get; } = IAppHost.GetService<RankingPageViewModel>();
    
    public RankingPage()
    {
        DataContext = this;
        InitializeComponent();

        PersonsDataGrid.Columns.AddRange(ViewModel.DataGridColumns);
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var search = ViewModel.SearchText;
        ViewModel.Persons.Clear();
        if (search == string.Empty)
        {
            ViewModel.Persons.AddRange(ViewModel.ProfileConfigHandler.Data.Profile.Persons);
            ViewModel.UpdatePersonWithStatusCountsList();
            return;
        }
        
        ViewModel.Persons.AddRange(ViewModel.ProfileConfigHandler.Data.Profile.Persons
            .Where(person => person.Value.IsMatch(search)));
        ViewModel.UpdatePersonWithStatusCountsList();
    }
    
    [RelayCommand]
    public void CopyStatusWithRanking(StatusWithRanking ranking)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        var text = ranking.Items
            .Aggregate(
                $"{ranking.Status.Name}：{ranking.Items.Count} 人上榜",
                (current, item) => current + $"\n{item.Person.Name} {item.Count} 次");

        topLevel.Clipboard.SetTextAsync(text).Wait();
        this.ShowSuccessToast("复制成功。");
    }

    private async void ButtonExport_OnClick(object? sender, RoutedEventArgs e)
    {
        var configData = ViewModel.ProfileConfigHandler.Data;
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var profileName = configData.Profile.Name;
        var today = DateTime.Today;
        var dateString = $"{today.Year}-{today.Month}-{today.Day} {today.Hour}-{today.Minute}";
        
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出 Excel 文件",
            SuggestedFileName = $"{profileName} 排行榜 {dateString}.xlsx",
            FileTypeChoices =
            [
                new FilePickerFileType("Excel 文件") { Patterns = ["*.xlsx"] }
            ]
        });
        if (file == null) return;
        
        var dt = new DataTable();
        
        dt.Columns.Add(new DataColumn("姓名", typeof(string)));
        dt.Columns.Add(new DataColumn("编号", typeof(string)));
        dt.Columns.Add(new DataColumn("性别", typeof(string)));
        dt.Columns.Add(new DataColumn("标签", typeof(string)));
        
        dt.Columns.AddRange(configData.Profile.Statuses
            .Select(kvp => new DataColumn(kvp.Value.Name, typeof(int)))
            .ToArray());
        
        foreach (var person in ViewModel.PersonWithStatusCountsList)
        {
            var row = dt.NewRow();

            row["姓名"] = person.Person.Name;
            row["编号"] = person.Person.Id;
            row["性别"] = person.Person.Sex switch
            {
                HumanSex.Male => "男",
                HumanSex.Female => "女",
                _ => "未知"
            };
            row["标签"] = person.Person.Tags.Aggregate(string.Empty, (current, item) =>
                current + configData.Profile.Tags.GetValueOrDefault(item, new Tag()).Name + ";");

            foreach (var (index, status) in configData.Profile.Statuses.Index())
            {
                row[status.Value.Name] = person.StatusCounts[index];
            }
            
            dt.Rows.Add(row);
        }
        
        await using var stream = await file.OpenWriteAsync();
        stream.SaveAs(dt, configuration: new OpenXmlConfiguration());
        
        this.ShowSuccessToast("导出成功。");
    }
}