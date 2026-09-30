using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TelltaleTextureTool.Utilities;

namespace TelltaleTextureTool.ViewModels;

public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = "File";
    public DateTime ModifiedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
}

public partial class ColumnVisibilitySettings : ObservableObject
{
    public IImage? Icon { get; set; }

    [ObservableProperty]
    public bool _isNameVisible = true;

    [ObservableProperty]
    public bool _isExtensionVisible = true;

    [ObservableProperty]
    public bool _isModifiedDateVisible = false;

    [ObservableProperty]
    public bool _isCreatedDateVisible = false;

    [ObservableProperty]
    public bool _isSizeVisible = false;

    [ObservableProperty]
    public bool _isMipSliderVisible = false;
}

public partial class FileExplorerViewModel : ObservableObject
{
    private FileSystemWatcher _watcher;

    public readonly ObservableCollection<FileItem> _allFiles = [];
    public ObservableCollection<FileItem> FilteredDirectoryFiles { get; } = [];
    public ColumnVisibilitySettings ColumnSettings { get; } = new();

    [ObservableProperty]
    private FileItem? _selectedItem;

    [ObservableProperty]
    private bool _contextOpenFolderStatus;

    [ObservableProperty]
    private string _currentDirectory;

    [ObservableProperty]
    private string _searchText = string.Empty;

    private void ApplySearchFilter()
    {
        FilteredDirectoryFiles.Clear();
        foreach (var item in _allFiles.Where(f => ShouldIncludeInSearch(f.Name)))
        {
            FilteredDirectoryFiles.Add(item);
        }
    }

    private bool ShouldIncludeInSearch(string name)
    {
        return string.IsNullOrEmpty(SearchText)
            || name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    public FileExplorerViewModel(string directory, IEnumerable<string> filters)
    {
        // Default directory
        CurrentDirectory = directory;
        SetupFileWatcher(CurrentDirectory, filters);

        // Initialize property changed handlers
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CurrentDirectory))
            {
                _watcher.Path = CurrentDirectory;
                LoadDirectoryFiles(CurrentDirectory);
                SearchText = string.Empty;
                ApplySearchFilter();
            }
            else if (e.PropertyName == nameof(SelectedItem))
            {
                ContextOpenFolderStatus = SelectedItem?.IsDirectory ?? false;
            }
            else if (e.PropertyName == nameof(SearchText))
            {
                ApplySearchFilter();
            }
        };

        LoadDirectoryFiles(CurrentDirectory);
        ApplySearchFilter();
    }

    private void SetupFileWatcher(string path, IEnumerable<string> filters)
    {
        _watcher?.Dispose();

        if (!Directory.Exists(path))
            return;

        _watcher = new FileSystemWatcher(path)
        {
            NotifyFilter =
                NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.Size
                | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.EnableRaisingEvents = true;

        SetFilters(filters);
    }

    public void SetFilters(IEnumerable<string> filters)
    {
        _watcher.Filters.Clear();

        foreach (var filter in filters)
        {
            _watcher.Filters.Add(filter);
        }
    }

    private void LoadDirectoryFiles(string path)
    {
        try
        {
            _allFiles.Clear();
            FilteredDirectoryFiles.Clear();

            if (!Directory.Exists(path))
                return;

            // Add directories first
            foreach (var dirPath in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dirPath);
                _allFiles.Add(
                    new FileItem
                    {
                        Name = dirInfo.Name,
                        Path = dirPath,
                        Size = 0,
                        CreatedDate = dirInfo.CreationTime,
                        ModifiedDate = dirInfo.LastWriteTime,
                        Type = "Folder",
                        IsDirectory = true,
                    }
                );
            }

            foreach (
                var filePath in Directory
                    .GetFiles(path)
                    .Where(f =>
                        string.IsNullOrEmpty(_watcher.Filters.FirstOrDefault())
                        || _watcher.Filters.Any(filter =>
                            f.EndsWith(filter, StringComparison.OrdinalIgnoreCase)
                        )
                    )
            )
            {
                var fileInfo = new FileInfo(filePath);
                _allFiles.Add(
                    new FileItem
                    {
                        Name = fileInfo.Name,
                        Path = filePath,
                        Size = fileInfo.Length,
                        CreatedDate = fileInfo.CreationTime,
                        ModifiedDate = fileInfo.LastWriteTime,
                        Type =
                            fileInfo.Extension.Length > 1
                                ? fileInfo.Extension[1..].ToUpperInvariant()
                                : string.Empty,
                        IsDirectory = false,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading directory: {ex.Message}");
        }
    }

    public void RowDoubleTapped(object? sender, TappedEventArgs args)
    {
        var source = args.Source;
        if (source is null)
            return;
        if (source is Border)
        {
            if (SelectedItem is null)
                return;

            var workingDirectoryFile = SelectedItem;

            var filePath = workingDirectoryFile.Path;

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                throw new DirectoryNotFoundException("Directory was not found");

            if (File.Exists(workingDirectoryFile.Path))
            {
                OpenFile(workingDirectoryFile.Path);
            }
            else
            {
                CurrentDirectory = workingDirectoryFile.Path;
            }
        }
    }

    [RelayCommand]
    private void ContextMenuOpenFile()
    {
        if (SelectedItem?.IsDirectory == false)
        {
            OpenFile(SelectedItem.Path);
        }
    }

    [RelayCommand]
    private void ContextMenuOpenFolder()
    {
        if (SelectedItem?.IsDirectory == true)
        {
            CurrentDirectory = SelectedItem.Path;
        }
    }

    [RelayCommand]
    private void ContextMenuOpenFileExplorer()
    {
        // var path = SelectedItem.IsDirectory
        //     ? SelectedItem.Path
        //     : Path.GetDirectoryName(SelectedItem.Path);

        // if (Directory.Exists(path))
        // {
        OpenFileExplorer(SelectedItem.Path);
        // }
    }

    public static void OpenFileExplorer(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"-R \"{filePath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", $"\"{Path.GetDirectoryName(filePath)}\"");
        }
        else
        {
            throw new NotSupportedException(
                "'Open File Explorer' is unsupported on this operating system.'"
            );
        }
    }

    [RelayCommand]
    private void AddFiles()
    {
        // This command can be used to open a file dialog and add files to the current directory
        // Implementation depends on the UI framework being used (e.g., Avalonia, WPF)
        // For example, you might use an OpenFileDialog to select files and then copy them to CurrentDirectory
    }

    [RelayCommand]
    private async Task DeleteFileAsync()
    {
        if (SelectedItem == null)
            return;

        if (SelectedItem.IsDirectory)
        {
            if (
                Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime lifetime
            )
            {
                var messageBox = MessageBoxes.GetConfirmationBox(
                    "Are you sure you want to delete this directory?"
                );

                var result = await MessageBoxManager
                    .GetMessageBoxStandard(messageBox)
                    .ShowWindowDialogAsync(lifetime.MainWindow);

                if (result is not ButtonResult.Yes)
                    return;

                Directory.Delete(SelectedItem.Path);
            }
        }
        else
        {
            File.Delete(SelectedItem.Path);
        }
    }

    private void OpenFile(string path)
    {
        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open",
                }
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening file: {ex.Message}");
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed && File.Exists(e.FullPath))
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var child in _allFiles!)
                {
                    if (child.Path == e.FullPath)
                    {
                        if (!child.IsDirectory)
                        {
                            var info = new FileInfo(e.FullPath);
                            child.Size = info.Length;
                            child.ModifiedDate = info.LastWriteTimeUtc;
                        }

                        break;
                    }
                }
            });
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Check if the extension matches the filters
            if (
                _watcher.Filters.Count == 0
                || _watcher.Filters.Any(filter =>
                    e.FullPath.EndsWith(filter, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return;
            }

            // TODO: Add Folder
            var node = new FileItem
            {
                Name = Path.GetFileName(e.FullPath),
                Path = e.FullPath,
                Type = Path.GetExtension(e.FullPath),
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDirectory = Directory.Exists(e.FullPath),
            };

            _allFiles!.Add(node);
        });
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            for (var i = 0; i < _allFiles!.Count; ++i)
            {
                if (_allFiles[i].Path == e.FullPath)
                {
                    _allFiles.RemoveAt(i);
                    Debug.WriteLine($"Removed {e.FullPath}");
                    break;
                }
            }
        });
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var child in _allFiles!)
            {
                if (child.Path == e.OldFullPath)
                {
                    child.Path = e.FullPath;
                    child.Name = e.Name ?? string.Empty;
                    break;
                }
            }
        });
    }

    [RelayCommand]
    public void ReturnDirectory()
    {
        if (string.IsNullOrEmpty(CurrentDirectory))
            return;

        var parentDirectory = Directory.GetParent(CurrentDirectory);
        if (parentDirectory != null)
        {
            CurrentDirectory = parentDirectory.FullName;
        }
    }
}
