using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PdfImageToolkit.Core.Models;
using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;

namespace PdfImageToolkit.ViewModels.Pdf;

public partial class RotateViewModel : ObservableObject
{
    private readonly IPdfService _pdfService;
    private readonly IFileService _fileService;
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    private CancellationTokenSource? _cts;

    public RotateViewModel(
        IPdfService pdfService,
        IFileService fileService,
        IEntitlementService entitlementService,
        ILoggingService loggingService)
    {
        _pdfService = pdfService;
        _fileService = fileService;
        _entitlementService = entitlementService;
        _loggingService = loggingService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFile))]
    [NotifyPropertyChangedFor(nameof(CanRotate))]
    [NotifyPropertyChangedFor(nameof(FileVisibility))]
    private FileItem? _selectedFile;

    [ObservableProperty]
    private int _rotationAngleIndex = 0; // 0 = 90°, 1 = 180°, 2 = 270°

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomRange))]
    [NotifyPropertyChangedFor(nameof(CustomRangeVisibility))]
    private int _pageScopeIndex = 0; // 0 = All pages, 1 = Custom range

    [ObservableProperty]
    private string _customRangeExpression = "1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRotate))]
    [NotifyPropertyChangedFor(nameof(BusyVisibility))]
    private bool _isBusy;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(ResultVisibility))]
    [NotifyPropertyChangedFor(nameof(RotatedFilePath))]
    private OperationResult? _result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool HasFile => SelectedFile != null;
    public bool CanRotate => SelectedFile != null && !IsBusy;
    public bool HasResult => Result != null && Result.Success;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsCustomRange => PageScopeIndex == 1;

    public Visibility FileVisibility => HasFile ? Visibility.Visible : Visibility.Collapsed;
    public Visibility CustomRangeVisibility => IsCustomRange ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ResultVisibility => HasResult ? Visibility.Visible : Visibility.Collapsed;
    public string RotatedFilePath => Result?.OutputFilePath ?? string.Empty;

    public int GetAngleDegrees() => RotationAngleIndex switch
    {
        1 => 180,
        2 => 270,
        _ => 90
    };

    public async Task SetFileAsync(string filePath)
    {
        ErrorMessage = null;
        Result = null;

        if (!Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Please select a valid PDF file.";
            return;
        }

        var item = new FileItem(filePath);

        // Check file size entitlement
        if (!_entitlementService.CanProcessFile(item.FileSizeBytes, out var fileReason))
        {
            ErrorMessage = fileReason;
            return;
        }

        SelectedFile = item;

        try
        {
            int count = await _pdfService.GetPageCountAsync(item.FilePath);
            item.PageCount = count;
            item.DetailText = $"{count} page{(count == 1 ? "" : "s")}";
            CustomRangeExpression = $"1-{count}";
        }
        catch (Exception ex)
        {
            item.DetailText = "Unable to read page count";
            _loggingService.LogError("Error getting page count", ex);
        }
    }

    [RelayCommand]
    private async Task PickFileAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            if (App.MainWindowHandle != 0)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindowHandle);
            }

            var picked = await picker.PickSingleFileAsync();
            if (picked != null)
            {
                await SetFileAsync(picked.Path);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error picking PDF file", ex);
        }
    }

    [RelayCommand]
    private void ClearFile()
    {
        SelectedFile = null;
        Result = null;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task ExecuteRotateAsync()
    {
        if (SelectedFile == null || IsBusy) return;

        if (!_entitlementService.TryConsumeOperation())
        {
            ErrorMessage = "You have reached your free daily operation limit (5/day). Upgrade to Pro for unlimited operations.";
            return;
        }

        IsBusy = true;
        Result = null;
        ErrorMessage = null;
        ProgressPercent = 0;
        StatusText = "Preparing rotation...";
        _cts = new CancellationTokenSource();

        try
        {
            var outputFolder = _fileService.GetDefaultOutputDirectory();
            var progress = new Progress<OperationProgress>(p =>
            {
                ProgressPercent = p.Percentage;
                StatusText = p.StatusMessage;
            });

            var range = PageScopeIndex == 1 ? CustomRangeExpression : null;
            var angle = GetAngleDegrees();

            var res = await _pdfService.RotatePdfAsync(
                SelectedFile.FilePath,
                angle,
                range,
                outputFolder,
                SelectedFile.Password,
                progress,
                _cts.Token);

            Result = res;
            if (!res.Success)
            {
                ErrorMessage = res.ErrorMessage;
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Rotation canceled.";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Unexpected error during rotation", ex);
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    private async Task OpenResultFileAsync()
    {
        if (Result?.OutputFilePath != null && File.Exists(Result.OutputFilePath))
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(Result.OutputFilePath);
            await Launcher.LaunchFileAsync(file);
        }
    }

    [RelayCommand]
    private async Task OpenResultFolderAsync()
    {
        var targetPath = Result?.OutputFilePath ?? Result?.OutputFilePaths?.FirstOrDefault();
        if (targetPath != null && File.Exists(targetPath))
        {
            var folderPath = Path.GetDirectoryName(targetPath);
            if (folderPath != null && Directory.Exists(folderPath))
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                await Launcher.LaunchFolderAsync(folder);
            }
        }
    }
}
