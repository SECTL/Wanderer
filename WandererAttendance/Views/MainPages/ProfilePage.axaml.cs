using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Platform.Storage;
using DynamicData;
using FluentAvalonia.UI.Controls;
using WandererAttendance.Abstraction;
using WandererAttendance.Attributes;
using WandererAttendance.Controls;
using WandererAttendance.Helpers.UI;
using WandererAttendance.Models;
using WandererAttendance.Models.Profile;
using WandererAttendance.Models.UI;
using WandererAttendance.ViewModels.MainPages;

namespace WandererAttendance.Views.MainPages;

[MainPageInfo("档案", "profile", "\uE081", true, true)]
public partial class ProfilePage : UserControl
{
    public ProfilePageViewModel ViewModel { get; } = IAppHost.GetService<ProfilePageViewModel>();

    public ProfilePage()
    {
        DataContext = this;
        InitializeComponent();

        ViewModel.PropertyChanged += (sender, args) =>
        {
            ButtonSwitchProfile.IsEnabled = ButtonCancelSwitchProfile.IsEnabled =
                ViewModel.SelectedProfile != ViewModel.CurrentProfile;
        };
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        ViewModel.ProfileConfigHandler.Save();
        DataContext = null;
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ProfileConfigHandler.Save();
        this.ShowSuccessToast("已保存。");
    }

    private async void ButtonCreateProfile_OnClick(object? sender, RoutedEventArgs e)
    {
        var textBox = new TextBox();
        var r = await new ContentDialog
        {
            Title = "新建档案",
            Content = new Field
            {
                Content = textBox,
                Label = "档案名称",
                Suffix = ".json"
            },
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "新建",
            SecondaryButtonText = "取消"
        }.ShowAsync();
        
        if (r != ContentDialogResult.Primary)
        {
            return;
        }

        var name = textBox.Text ?? "EMPTY";
        var path = Path.Combine(Services.ProfileService.ProfilePath, $"{name}.json");

        if (File.Exists(path))
        {
            this.ShowWarningToast("重复的档案名称。");
            return;
        }

        var profile = new ProfileConfigModel(name);
        profile.Profile.Statuses.AddRange(GlobalConstants.DefaultStatuses);
        
        var json = JsonSerializer.Serialize(profile);
        await File.WriteAllTextAsync(path, json);
        
        ViewModel.RefreshProfiles();
        ViewModel.SelectedProfile = name;
    }

    private void ButtonRefreshProfiles_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.RefreshProfiles();
    }

    private async void MenuItemRenameProfile_OnClick(object? sender, RoutedEventArgs e)
    {
        var textBox = new TextBox
        {
            Text = ViewModel.SelectedProfile
        };
        var r = await new ContentDialog
        {
            Title = "重命名档案",
            Content = new Field
            {
                Content = textBox,
                Label = "档案名称",
                Suffix = ".json"
            },
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "重命名",
            SecondaryButtonText = "取消"
        }.ShowAsync();

        var raw = Path.Combine(Services.ProfileService.ProfilePath, $"{ViewModel.SelectedProfile}.json");
        var path = Path.Combine(Services.ProfileService.ProfilePath, $"{textBox.Text}.json");
        if (r != ContentDialogResult.Primary || !File.Exists(raw))
        {
            return;
        }

        if (File.Exists(path))
        {
            this.ShowToast(new ToastMessage
            {
                Message = "无法重命名档案，因为已存在一个相同名称的档案。",
                Severity = InfoBarSeverity.Warning
            });
            return;
        }

        File.Move(raw, path);
        if (Services.ProfileService.ProfileName == Path.GetFileNameWithoutExtension(raw))
        {
            Services.ProfileService.ProfileName = Path.GetFileNameWithoutExtension(path);
            ViewModel.MainConfigHandler.Data.ProfileName = Path.GetFileNameWithoutExtension(path);
        }

        // rename
        var text = await File.ReadAllTextAsync(path);
        var profile = JsonSerializer.Deserialize<ProfileConfigModel>(text) ?? new ProfileConfigModel();
        profile.Profile.Name = textBox.Text;
        var json = JsonSerializer.Serialize(profile);
        await File.WriteAllTextAsync(path, json);
        
        ViewModel.RefreshProfiles();
        ViewModel.SelectedProfile = textBox.Text;
    }

    private async void MenuItemProfileDuplicate_OnClick(object? sender, RoutedEventArgs e)
    {
        var raw = Path.Combine(Services.ProfileService.ProfilePath, $"{ViewModel.SelectedProfile}.json");
        var d = $"{ViewModel.SelectedProfile} - 副本.json";
        var d1 = Path.Combine(Services.ProfileService.ProfilePath, $"{d}");
        File.Copy(raw, d1);
        
        // rename
        var text = await File.ReadAllTextAsync(d1);
        var profile = JsonSerializer.Deserialize<ProfileConfigModel>(text) ?? new ProfileConfigModel();
        profile.Profile.Name = Path.GetFileNameWithoutExtension(d);
        var json = JsonSerializer.Serialize(profile);
        await File.WriteAllTextAsync(d1, json);
        
        ViewModel.RefreshProfiles();
        ViewModel.SelectedProfile = Path.GetFileNameWithoutExtension(d);
    }

    private async void MenuItemDeleteProfile_OnClick(object? sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Services.ProfileService.ProfilePath, $"{ViewModel.SelectedProfile}.json");
        if (ViewModel.SelectedProfile == Services.ProfileService.ProfileName)
        {
            this.ShowToast(new ToastMessage("无法删除已加载的档案。")
            {
                Severity = InfoBarSeverity.Warning
            });
            return;
        }

        var r = await new ContentDialog
        {
            Title = "删除档案",
            Content = $"您确定要删除档案 {ViewModel.SelectedProfile} 吗？此操作无法撤销，档案内的信息都将被删除！",
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "删除",
            SecondaryButtonText = "取消"
        }.ShowAsync();

        if (r == ContentDialogResult.Primary)
        {
            File.Delete(path);
        }

        ViewModel.SelectedProfile = ViewModel.CurrentProfile;
        ViewModel.RefreshProfiles();
    }

    private void ButtonCancelSwitchProfile_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SelectedProfile = ViewModel.CurrentProfile;
        ViewModel.RefreshProfiles();
    }

    private async void ButtonSwitchProfile_OnClick(object? sender, RoutedEventArgs e)
    {
        var r = await new ContentDialog
        {
            Title = "切换档案",
            Content = $"您确定要切换到档案 {ViewModel.SelectedProfile} 吗？可能会未保存的信息！",
            DefaultButton = ContentDialogButton.Secondary,
            PrimaryButtonText = "确定",
            SecondaryButtonText = "取消"
        }.ShowAsync();

        if (r == ContentDialogResult.Secondary)
        {
            return;
        }
        
        Services.ProfileService.ProfileName = ViewModel.SelectedProfile;
        ViewModel.MainConfigHandler.Data.ProfileName = ViewModel.SelectedProfile;
        ViewModel.ProfileConfigHandler.Reload();
        ViewModel.RefreshProfiles();
        IAppHost.GetService<MainView>().SelectNavigationItemById("profile");
    }

    private void ButtonAddPerson_OnClick(object? sender, RoutedEventArgs e)
    {
        var person = new Person();
        ViewModel.ProfileConfigHandler.Data.Profile.Persons.Add(person);
        ViewModel.SelectedPerson = person;
    }

    private void ButtonRemovePerson_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedPerson == null)
        {
            return;
        }

        var person = ViewModel.SelectedPerson;
        ViewModel.ProfileConfigHandler.Data.Profile.Persons.Remove(person);
        ViewModel.SelectedPerson = null;

        var revertButton = new Button { Content = "撤销" };
        var toastMessage = new ToastMessage($"已删除「{person.Name}」。")
        {
            ActionContent = revertButton,
            Duration = TimeSpan.FromSeconds(10)
        };

        revertButton.Click += (_, _) =>
        {
            ViewModel.ProfileConfigHandler.Data.Profile.Persons.Add(person);
            ViewModel.SelectedPerson = person;
            toastMessage.Close();
        };

        this.ShowToast(toastMessage);
    }

    private async void ButtonImportPersons_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        // 打开文件选择对话框
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择学生名单文件",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("所有支持格式") { Patterns = ["*.xlsx", "*.xls", "*.csv", "*.txt"] },
                new FilePickerFileType("Excel 文件") { Patterns = ["*.xlsx", "*.xls"] },
                new FilePickerFileType("CSV 文件") { Patterns = ["*.csv"] },
                new FilePickerFileType("文本文件") { Patterns = ["*.txt"] }
            ]
        });
        
        if (files.Count == 0) return;
        var file = files[0];
        
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (!new[] {".txt", ".xlsx", ".xls", ".csv"}.Contains(extension))
        {
            this.ShowErrorToast("不支持的文件格式");
            return;
        }
        
        await using var stream = await file.OpenReadAsync();

        ViewModel.Sheet = extension switch
        {
            ".txt" => LoadFromTxt(stream),
            ".xlsx" or ".xls" => LoadFromExcel(stream),
            ".csv" => LoadFromCsv(stream),
            _ => []
        };
        
        ViewModel.PreProcessPersons();
        ViewModel.ProcessPersons();
        
        if (this.FindResource("ImportSheetDataControl") is not ContentControl cc) return;
        cc.DataContext = this;
        
        if (cc.Parent is ContentDialog contentDialog)
        {
            contentDialog.Content = null;
        }
        
        var dialog = new ContentDialog
        {
            Content = cc,
            TitleTemplate = new DataTemplate(),
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "确定",
            SecondaryButtonText = "取消",
            DataContext = this
        };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            ViewModel.ProfileConfigHandler.Data.Profile.Persons.AddRange(ViewModel.ImportedPersons);
        }
    }

    private void ButtonApplySettings_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ProcessPersons();
    }

    private void ButtonAddStatus_OnClick(object? sender, RoutedEventArgs e)
    {
        var status = new Status();
        ViewModel.ProfileConfigHandler.Data.Profile.Statuses.Add(status);
        ViewModel.SelectedStatus = status;
    }

    private void ButtonRemoveStatus_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedStatus == null)
        {
            return;
        }

        var status = ViewModel.SelectedStatus;
        ViewModel.ProfileConfigHandler.Data.Profile.Statuses.Remove(status);
        ViewModel.SelectedStatus = null;

        var revertButton = new Button { Content = "撤销" };
        var toastMessage = new ToastMessage($"已删除「{status.Name}」。")
        {
            ActionContent = revertButton,
            Duration = TimeSpan.FromSeconds(10)
        };

        revertButton.Click += (_, _) =>
        {
            ViewModel.ProfileConfigHandler.Data.Profile.Statuses.Add(status);
            ViewModel.SelectedStatus = status;
            toastMessage.Close();
        };

        this.ShowToast(toastMessage);
    }
}
