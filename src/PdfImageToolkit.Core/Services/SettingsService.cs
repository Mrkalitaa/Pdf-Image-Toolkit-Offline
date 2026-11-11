using System;
using System.IO;
using System.Text.Json;

namespace PdfImageToolkit.Core.Services;

public interface ISettingsService
{
    string DefaultOutputFolder { get; set; }
    string AppTheme { get; set; }
    bool StripMetadataDefault { get; set; }
    bool SimulatedPro { get; set; }
    void Save();
    void Load();
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;

    public string DefaultOutputFolder { get; set; } = string.Empty;
    public string AppTheme { get; set; } = "Default";
    public bool StripMetadataDefault { get; set; } = false;
    public bool SimulatedPro { get; set; } = false;

    public SettingsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PdfImageToolkit");

        try
        {
            Directory.CreateDirectory(dir);
        }
        catch
        {
            // Ignore
        }

        _settingsFilePath = Path.Combine(dir, "settings.json");
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                if (data != null)
                {
                    DefaultOutputFolder = data.DefaultOutputFolder ?? string.Empty;
                    AppTheme = data.AppTheme ?? "Default";
                    StripMetadataDefault = data.StripMetadataDefault;
                    SimulatedPro = data.SimulatedPro;
                }
            }
        }
        catch
        {
            // Fall back to defaults
        }
    }

    public void Save()
    {
        try
        {
            var data = new SettingsData
            {
                DefaultOutputFolder = DefaultOutputFolder,
                AppTheme = AppTheme,
                StripMetadataDefault = StripMetadataDefault,
                SimulatedPro = SimulatedPro
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
            // Suppress save errors
        }
    }

    private class SettingsData
    {
        public string? DefaultOutputFolder { get; set; }
        public string? AppTheme { get; set; }
        public bool StripMetadataDefault { get; set; }
        public bool SimulatedPro { get; set; }
    }
}
