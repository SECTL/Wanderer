using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using WandererAttendance.Enums;
using WandererAttendance.Models.Profile;
using WandererAttendance.Services;
using WandererAttendance.Services.Config;

namespace WandererAttendance.ViewModels.MainPages;

public partial class ProfilePageViewModel : ObservableRecipient
{
    public partial class ColumnInfo : ObservableRecipient
    {
        [ObservableProperty] private string _header = string.Empty;
        [ObservableProperty] private int _index = 0;
    }

    public record HeaderItem
    {
        public string HeaderText { get; init; } = string.Empty;
        public string IndexText { get; init; } = string.Empty;
    }
    
    public MainConfigHandler MainConfigHandler { get; }
    public ProfileConfigHandler ProfileConfigHandler { get; }
    public ProfileService ProfileService { get; }

    [ObservableProperty] private int _selectedPage = 0;

    // 切换档案
    [ObservableProperty] private string _currentProfile;
    [ObservableProperty] private string _selectedProfile;
    
    // 人员编辑
    [ObservableProperty] private Person? _selectedPerson;

    // 导入人员
    [ObservableProperty] private bool _isImporting = false;
    [ObservableProperty] private ObservableCollection<HeaderItem> _sheetHeaders = [];
    [ObservableProperty] private List<List<string>> _sheet = [];
    
    [ObservableProperty] private bool _hasSheetHeader = false;
    [ObservableProperty] private bool _hasNameColumn = false;
    [ObservableProperty] private bool _hasIdColumn = false;
    [ObservableProperty] private bool _hasSexColumn = false;
    
    [ObservableProperty] private ColumnInfo _nameColumnInfo = new();
    [ObservableProperty] private ColumnInfo _idColumnInfo = new();
    [ObservableProperty] private ColumnInfo _sexColumnInfo = new();
    
    [ObservableProperty] private ObservableCollection<Person> _importedPersons = [];
    [ObservableProperty] private ObservableCollection<Person> _previewPersons = [];
    
    // 状态编辑
    [ObservableProperty] private Status? _selectedStatus;
    
    public ProfilePageViewModel(MainConfigHandler mainConfigHandler, ProfileConfigHandler profileConfigHandler, ProfileService profileService)
    {
        MainConfigHandler = mainConfigHandler;
        ProfileConfigHandler = profileConfigHandler;
        ProfileService = profileService;

        CurrentProfile = SelectedProfile = ProfileService.ProfileName;
    }
    
    public void RefreshProfiles()
    {
        var aaa = SelectedProfile;
        ProfileService.RefreshProfiles();
        CurrentProfile = ProfileService.ProfileName;
        SelectedProfile = aaa;
    }
    
    private void Cleanup()
    {
        HasSheetHeader = false;
        HasNameColumn = false;
        HasIdColumn = false;
        HasSexColumn = false;

        NameColumnInfo = new ColumnInfo();
        IdColumnInfo = new ColumnInfo();
        SexColumnInfo = new ColumnInfo();
        
        SheetHeaders.Clear();
        ImportedPersons.Clear();
    }
    
    public void PreProcessPersons()
    {
        Cleanup();
        
        // 判断是否含有表头
        List<List<string>> headerChoices =
        [
            GlobalConstants.ImportSheetStaticTexts.NameHeaderTexts,
            GlobalConstants.ImportSheetStaticTexts.IdHeaderTexts,
            GlobalConstants.ImportSheetStaticTexts.SexHeaderTexts
        ];
        foreach (var choice in from list in headerChoices from choice in list select choice)
        {
            if (!Sheet[0].Any(item => item.Contains(choice))) continue;
            
            HasSheetHeader = true;
            break;
        }

        // 表头
        for (var i = 0; i < Sheet[0].Count; i++)
        {
            SheetHeaders.Add(new HeaderItem
            {
                HeaderText = Sheet[0][i],
                IndexText = $"第 {i} 列"
            });
        }

        if (!HasSheetHeader)
        {
            // 晚会再做自动识别列内容
            return;
        }
        
        // 识别列标题
        for (var i = 0; i < Sheet[0].Count; i++)
        {
            foreach (var choice in GlobalConstants.ImportSheetStaticTexts.NameHeaderTexts.Where(choice => Sheet[0][i] == choice))
            {
                HasNameColumn = true;
                NameColumnInfo.Header = choice;
                NameColumnInfo.Index = i;
                break;
            }
            
            foreach (var choice in GlobalConstants.ImportSheetStaticTexts.IdHeaderTexts.Where(choice => Sheet[0][i] == choice))
            {
                HasIdColumn = true;
                IdColumnInfo.Header = choice;
                IdColumnInfo.Index = i;
                break;
            }

            foreach (var choice in GlobalConstants.ImportSheetStaticTexts.SexHeaderTexts.Where(choice => Sheet[0][i] == choice))
            {
                HasSexColumn = true;
                SexColumnInfo.Header = choice;
                SexColumnInfo.Index = i;
                break;
            }
        }
    }
    
    public void ProcessPersons()
    {
        if (!HasNameColumn && !HasIdColumn)
        {
            return;
        }
        
        ImportedPersons.Clear();

        for (var i = 0; i < Sheet.Count; i++)
        {
            // 跳过表头
            if (HasSheetHeader && i == 0)
            {
                continue;
            }
            
            // 跳过空行
            var line = Sheet[i];
            if (line.Count == 0 || line is [""])
            {
                return;
            }
            
            // 识别字段
            var name = string.Empty;
            var id = string.Empty;
            var sex = HumanSex.Unknown;
            
            if (HasNameColumn)
            {
                name = line[NameColumnInfo.Index];
            }

            if (HasIdColumn)
            {
                id = line[IdColumnInfo.Index];
            }

            if (HasSexColumn)
            {
                if (GlobalConstants.ImportSheetStaticTexts.SexTexts.Male.Any(choice => line[SexColumnInfo.Index] == choice))
                {
                    sex = HumanSex.Male;
                }
                else if (GlobalConstants.ImportSheetStaticTexts.SexTexts.Female.Any(choice => line[SexColumnInfo.Index] == choice))
                {
                    sex = HumanSex.Female;
                }
            }
            
            // 添加
            ImportedPersons.Add(new Person
            {
                Name = name,
                Id = id,
                Sex = sex
            });
        }
        
        PreviewPersons.Clear();
        PreviewPersons.AddRange(ImportedPersons.Take(5));
    }
}
