using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelltaleTextureTool.ViewModels;

public partial class AppConfig
{
    public string LastFolder { get; set; } = string.Empty;
    public string Theme { get; set; } = "Light";

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TelltaleTextureTool",
        "config.json"
    );

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }

        return new AppConfig();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}
