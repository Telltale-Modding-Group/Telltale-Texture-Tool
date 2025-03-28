using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using TelltaleTextureTool.Utilities;

namespace TelltaleTextureTool;

/// <summary>
/// Handles main application functions and activity.
/// MainManager is a singleton, meaning only one instance of the class exists.
/// </summary>
public sealed class MainManager
{
    // App version
    public readonly string AppVersion = "v2.5.2";

    // Weblink for getting help with the application
    private const string AppHelpLink =
        "https://github.com/Telltale-Modding-Group/Telltale-Texture-Tool/wiki";

    private WorkingDirectory _workingDirectory;
    private static MainManager? _instance;

    private MainManager()
    {
        //create the rest of our objects
        _workingDirectory = new WorkingDirectory();
    }

    public WorkingDirectory GetWorkingDirectory() => _workingDirectory;

    public static MainManager GetInstance() => _instance ??= new MainManager();

    /// <summary>
    /// Sets the current working directory path using a folder picker.
    /// </summary>
    /// <param name="provider"></param>
    public void SetWorkingDirectoryPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        _workingDirectory.GetFiles(path, _workingDirectory.FilterFileExtensions);
    }

    /// <summary>
    /// Opens a file using its preferred software.
    /// </summary>
    /// <param name="directoryPath"></param>
    public void OpenFile(string? directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            return;

        //create a windows explorer processInfo to be executed
        ProcessStartInfo processStartInfo = new()
        {
            FileName = directoryPath,
            UseShellExecute = true,
            Verb = "open",
        };

        //start the process
        Process.Start(processStartInfo);
    }

    /// <summary>
    /// Opens the file explorer.
    /// </summary>
    /// <param name="directoryPath"></param>
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

    /// <summary>
    /// Opens the default web explorer and directs the user to the help page.
    /// </summary>
    public static void OpenAppHelp()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = AppHelpLink,
            UseShellExecute = true,
        };

        Process.Start(processStartInfo);
    }

    /// <summary>
    /// Refreshes the current directory, in case the user adds or deletes files using other software.
    /// </summary>
    public void RefreshWorkingDirectory()
    {
        _workingDirectory.GetFiles(
            _workingDirectory.SelectedDirectory.FullName,
            _workingDirectory.FilterFileExtensions
        );
    }

    public string GetWorkingDirectoryPath() => _workingDirectory.SelectedDirectory.FullName;

    public List<FileSystemInfo> GetWorkingDirectoryFiles() => _workingDirectory.SelectedItems;
}
