using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.VisualBasic.FileIO;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using XCleaner.Helpers;
using XCleaner.Models;
using XCleaner.Services;

namespace XCleaner.ViewModels.Pages;

public partial class CleanerViewModel : ObservableObject
{
    [ObservableProperty] private CleanFolder _selectedFolder = CleanFolder.Local;
    private readonly CleanerService _service;
    private readonly IContentDialogService _contentDialogService;
    private readonly ICollectionView _itemsView;
    private readonly HashSet<CleanItem> _selectedItems = [];
    private List<CleanItem> _deleteItems = null!;
    private bool _isUpdatingChecked;
    public IReadOnlyList<CleanFolder> CleanFolders { get; } = Enum.GetValues<CleanFolder>();
    public ObservableCollection<CleanItem> Items { get; } = [];

    public CleanerViewModel(CleanerService service, IContentDialogService contentDialogService)
    {
        _service = service;
        _contentDialogService = contentDialogService;
        _itemsView = CollectionViewSource.GetDefaultView(Items);
    }

    [RelayCommand]
    private async Task Scan()
    {
        _selectedItems.Clear();
        Items.Clear();
        var progress = new Progress<CleanItem>(item => { Items.Add(item); });
        await _service.ScanAsync(GetCleanFolderPath(), progress);
    }

    [RelayCommand]
    private void SortByName()
    {
        _itemsView.SortDescriptions.Clear();
        _itemsView.SortDescriptions.Add(new SortDescription(nameof(CleanItem.Name), ListSortDirection.Ascending));
    }

    [RelayCommand]
    private void SortByModified()
    {
        _itemsView.SortDescriptions.Clear();
        _itemsView.SortDescriptions.Add(new SortDescription(nameof(CleanItem.LatestFileModifiedTime),
            ListSortDirection.Descending));
    }

    [RelayCommand]
    private void SortBySize()
    {
        _itemsView.SortDescriptions.Clear();
        _itemsView.SortDescriptions.Add(new SortDescription(nameof(CleanItem.Size), ListSortDirection.Descending));
    }

    #region Delete

    [RelayCommand]
    private async Task OnShowDialog(object content)
    {
        _deleteItems = Items.Where(x => x.Checked).ToList();
        if (_deleteItems.Count == 0)
        {
            return;
        }

        var result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
            {
                Title = "确定要批量删除已选中的目录吗？",
                Content = $"当前已选中{_deleteItems.Count}个目录",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消"
            }
        );
        if (result == ContentDialogResult.Primary)
        {
            await DeleteCheckedItems();
        }
    }

    private async Task DeleteCheckedItems()
    {
        foreach (var cleanItem in _deleteItems)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(cleanItem.Path))
                    {
                        FileSystem.DeleteDirectory(cleanItem.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Delete Failed, {cleanItem.Path}: {ex.Message}");
                }
            });
            _selectedItems.Remove(cleanItem);
            Items.Remove(cleanItem);
        }
    }

    #endregion

    [RelayCommand]
    private void SelectionChanged(SelectionChangedEventArgs e)
    {
        foreach (var item in e.AddedItems)
        {
            if (item is CleanItem cleanItem)
            {
                _selectedItems.Add(cleanItem);
            }
        }

        foreach (var item in e.RemovedItems)
        {
            if (item is CleanItem cleanItem)
            {
                _selectedItems.Remove(cleanItem);
            }
        }
    }

    [RelayCommand]
    private void CheckChanged(bool value)
    {
        if (_isUpdatingChecked || _selectedItems.Count <= 1)
        {
            return;
        }

        _isUpdatingChecked = true;
        foreach (var cleanItem in _selectedItems)
        {
            cleanItem.Checked = value;
        }

        _isUpdatingChecked = false;
    }

    [RelayCommand]
    private void ShowInExplorer(object param)
    {
        ExplorerHelper.FilesOrFolders(_selectedItems.Select(cleanItem => cleanItem.Path));
    }

    private string GetCleanFolderPath()
    {
        return SelectedFolder switch
        {
            CleanFolder.Local => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            CleanFolder.Roaming => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            CleanFolder.LocalLow => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow"),
            CleanFolder.ProgramData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}