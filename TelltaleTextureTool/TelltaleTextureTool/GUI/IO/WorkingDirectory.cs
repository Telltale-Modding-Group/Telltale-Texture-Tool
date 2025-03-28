using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace TelltaleTextureTool;

public class WorkingDirectory
{
    public DirectoryInfo SelectedDirectory = new(Environment.SystemDirectory);
    public List<FileSystemInfo> SelectedItems = [];
    public IEnumerable<string> FilterFileExtensions = [];

    /// <summary>
    /// Gets the files from the provided directory path.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public ObservableCollection<FileSystemItem> GetFiles(
        string directoryPath,
        IEnumerable<string> filterExtensions
    )
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException("Selected directory cannot be found.");
        }

        if (directoryPath != SelectedDirectory.FullName)
        {
            SelectedDirectory = new DirectoryInfo(directoryPath);
            SelectedItems = [];
            FilterFileExtensions = filterExtensions;
        }

        SelectedItems = [];

        SelectedItems = [.. SelectedItems.Concat(SelectedDirectory.GetDirectories())];

        foreach (var item in FilterFileExtensions)
        {
            SelectedItems = [.. SelectedItems.Concat(SelectedDirectory.GetFiles("*" + item))];
        }

        return [.. SelectedItems.Select(CreateFileSystemItem)];
    }

    private FileSystemItem CreateFileSystemItem(FileSystemInfo info)
    {
        return new FileSystemItem
        {
            // Icon = _iconService.GetIcon(info.FullName),
            Name = info.Name,
            FullPath = info.FullName,
            Type = info is DirectoryInfo ? "Folder" : info.Extension,
            ModifiedDate = info.LastWriteTime,
            CreatedDate = info.CreationTime,
            IsDirectory = info is DirectoryInfo,
            Size = info is FileInfo file ? file.Length : 0,
        };
    }

    /// <summary>
    /// Gets the files from the provided directory path.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public ObservableCollection<FileSystemItem> UpdateFiles(
        ObservableCollection<FileSystemItem> files
    )
    {
        var updatedFiles = GetFiles(SelectedDirectory.FullName, FilterFileExtensions);

        // Update existing files and remove deleted ones
        for (int i = files.Count - 1; i >= 0; i--) // Iterate backwards for safe removal
        {
            var file = files[i];
            var matchingUpdatedFile = updatedFiles.FirstOrDefault(x => x.FullPath == file.FullPath);

            if (matchingUpdatedFile != null)
            {
                // Update existing file metadata
                file.CreatedDate = matchingUpdatedFile.CreatedDate;
                file.ModifiedDate = matchingUpdatedFile.ModifiedDate;
            }
            else if (!File.Exists(file.FullPath))
            {
                // Remove files that no longer exist
                files.RemoveAt(i);
            }
        }

        // Add new files not already in the collection
        var newFiles = updatedFiles
            .Where(uf => !files.Any(f => f.FullPath == uf.FullPath))
            .ToList();

        foreach (var newFile in newFiles)
        {
            files.Add(CreateFileSystemItem(new FileInfo(newFile.FullPath)));
        }

        return files;
    }
}
