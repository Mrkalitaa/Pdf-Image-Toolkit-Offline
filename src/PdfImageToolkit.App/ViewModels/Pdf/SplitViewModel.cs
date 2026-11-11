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

public partial class SplitViewModel : ObservableObject
{
    private readonly IPdfService _pdfService;
    private readonly IFileService _fileService;
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    private CancellationTokenSource? _cts;

    public SplitViewModel(
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
    [NotifyPropertyChangedFor(nameof(CanSplit))]
    [NotifyPropertyChangedFor(nameof(FileVisibility))]
    private FileItem? _selectedFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRangeMode))]
    [NotifyPropertyChangedFor(nameof(IsEveryPageMode))]
    [NotifyPropertyChangedFor(nameof(IsEveryNPagesMode))]
    [NotifyPropertyChangedFor(nameof(RangeModeVisibility))]
    [NotifyPropertyChangedFor(nameof(EveryNPagesVisibility))]
    private int _splitModeIndex = 0; // 0 = Range, 1 = Every page, 2 = Every N pages

    [ObservableProperty]
    private string _rangeExpression = "1";

    [ObservableProperty]
    private int _pagesPerSplit = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSplit))]
    [NotifyPropertyChangedFor(nameof(BusyVisibility))]
    private bool _isBusy;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(ResultVisibility))]
    [NotifyPropertyChangedFor(nameof(ResultSummaryText))]
    [NotifyPropertyChangedFor(nameof(OpenFileButtonVisibility))]
    private OperationResult? _result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool HasFile => SelectedFile != null;
    public bool CanSplit => SelectedFile != null && !IsBusy;
    public bool HasResult => Result != null && Result.Success;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsRangeMode => SplitModeIndex == 0;
    public bool IsEveryPageMode => SplitModeIndex == 1;
    public bool IsEveryNPagesMode => SplitModeIndex == 2;

    public Visibility FileVisibility => HasFile ? Visibility.Visible : Visibility.Collapsed;
    public Visibility RangeModeVisibility => IsRangeMode ? Visibility.Visible : Visibility.Collapsed;
    public Visibility EveryNPagesVisibility => IsEveryNPagesMode ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ResultVisibility => HasResult ? Visibility.Visible : Visibility.Collapsed;

    public string ResultSummaryText => Result?.OutputFilePath ?? "Multiple files exported to destination folder.";
    public Visibility OpenFileButtonVisibility => Result?.OutputFilePath != null ? Visibility.Visible : Visibility.Collapsed;

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
            RangeExpression = $"1-{Math.Min(count, 3)}";
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
    private async Task ExecuteSplitAsync()
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
        StatusText = "Preparing split...";
        _cts = new CancellationTokenSource();

        try
        {
            var outputFolder = _fileService.GetDefaultOutputDirectory();
            var progress = new Progress<OperationProgress>(p =>
            {
                ProgressPercent = p.Percentage;
                StatusText = p.StatusMessage;
            });

            OperationResult res;
            if (SplitModeIndex == 0)
            {
                res = await _pdfService.SplitPdfAsync(
                    SelectedFile.FilePath,
                    RangeExpression,
                    outputFolder,
                    SelectedFile.Password,
                    progress,
                    _cts.Token);
            }
            else if (SplitModeIndex == 1)
            {
                res = await _pdfService.SplitEveryPageAsync(
                    SelectedFile.FilePath,
                    outputFolder,
                    SelectedFile.Password,
                    progress,
                    _cts.Token);
            }
            else
            {
                res = await _pdfService.SplitEveryNPagesAsync(
                    SelectedFile.FilePath,
                    Math.Max(1, PagesPerSplit),
                    outputFolder,
                    SelectedFile.Password,
                    progress,
                    _cts.Token);
            }

            Result = res;
            if (!res.Success)
            {
                ErrorMessage = res.ErrorMessage;
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Split canceled.";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Unexpected error during split", ex);
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
