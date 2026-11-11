using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace PdfImageToolkit.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    public SettingsViewModel(
        ISettingsService settingsService,
        IEntitlementService entitlementService,
        ILoggingService loggingService)
    {
        _settingsService = settingsService;
        _entitlementService = entitlementService;
        _loggingService = loggingService;

        _outputFolder = _settingsService.DefaultOutputFolder;
        _selectedThemeIndex = _settingsService.AppTheme switch
        {
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };
        _stripMetadataDefault = _settingsService.StripMetadataDefault;
        _simulatedPro = _settingsService.SimulatedPro;
        _logFilePath = _loggingService.GetLogFilePath();
    }

    [ObservableProperty]
    private string _outputFolder;

    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private bool _stripMetadataDefault;

    [ObservableProperty]
    private bool _simulatedPro;

    [ObservableProperty]
    private string _logFilePath;

    public string AppVersion => "v1.0.0 (Production Build)";

    public event Action<string>? ThemeChanged;

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "Default"
        };
        _settingsService.AppTheme = theme;
        _settingsService.Save();
        ThemeChanged?.Invoke(theme);
    }

    partial void OnStripMetadataDefaultChanged(bool value)
    {
        _settingsService.StripMetadataDefault = value;
        _settingsService.Save();
    }

    partial void OnSimulatedProChanged(bool value)
    {
        _settingsService.SimulatedPro = value;
        _settingsService.Save();
        _entitlementService.SetSimulatedPro(value);
    }

    [RelayCommand]
    private async Task ChooseOutputFolderAsync()
    {
        try
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            
            if (App.MainWindowHandle != 0)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.MainWindowHandle);
            }

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                OutputFolder = folder.Path;
                _settingsService.DefaultOutputFolder = folder.Path;
                _settingsService.Save();
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Failed to pick folder", ex);
        }
    }

    [RelayCommand]
    private void ResetOutputFolder()
    {
        OutputFolder = string.Empty;
        _settingsService.DefaultOutputFolder = string.Empty;
        _settingsService.Save();
    }
}
