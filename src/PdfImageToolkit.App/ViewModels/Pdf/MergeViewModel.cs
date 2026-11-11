using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PdfImageToolkit.Core.Models;
using PdfImageToolkit.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;

namespace PdfImageToolkit.ViewModels.Pdf;

public partial class MergeViewModel : ObservableObject
{
    private readonly IPdfService _pdfService;
    private readonly IFileService _fileService;
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    private CancellationTokenSource? _cts;

    public MergeViewModel(
        IPdfService pdfService,
        IFileService fileService,
        IEntitlementService entitlementService,
        ILoggingService loggingService)
    {
        _pdfService = pdfService;
        _fileService = fileService;
        _entitlementService = entitlementService;
        _loggingService = loggingService;
        Files = [];
    }

    public ObservableCollection<FileItem> Files { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedFile))]
    [NotifyPropertyChangedFor(nameof(CanMoveSelectedUp))]
    [NotifyPropertyChangedFor(nameof(CanMoveSelectedDown))]
    private FileItem? _selectedFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanMerge))]
    [NotifyPropertyChangedFor(nameof(HasFiles))]
    [NotifyPropertyChangedFor(nameof(FilesVisibility))]
    [NotifyPropertyChangedFor(nameof(BusyVisibility))]
    private bool _isBusy;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(ResultVisibility))]
    [NotifyPropertyChangedFor(nameof(MergedFilePath))]
    private OperationResult? _result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool HasFiles => Files.Count > 0;
    public bool HasSelectedFile => SelectedFile != null;
    public bool CanMoveSelectedUp => SelectedFile != null && Files.IndexOf(SelectedFile) > 0;
    public bool CanMoveSelectedDown => SelectedFile != null && Files.IndexOf(SelectedFile) < Files.Count - 1;
    public bool CanMerge => Files.Count >= 2 && !IsBusy;
    public bool HasResult => Result != null && Result.Success;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
    public string MergedFilePath => Result?.OutputFilePath ?? string.Empty;

    public Visibility FilesVisibility => HasFiles ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ResultVisibility => HasResult ? Visibility.Visible : Visibility.Collapsed;

    public async Task AddFilesAsync(IEnumerable<string> paths)
    {
        ErrorMessage = null;
        var validPdfPaths = paths.Where(p => Path.GetExtension(p).Equals(".pdf", StringComparison.OrdinalIgnoreCase)).ToList();

        if (validPdfPaths.Count == 0) return;

        // Check batch size entitlement
        int projectedTotal = Files.Count + validPdfPaths.Count;
        if (!_entitlementService.CanProcessBatch(projectedTotal, out var batchReason))
        {
            ErrorMessage = batchReason;
            return;
        }

        foreach (var path in validPdfPaths)
        {
            var fileItem = new FileItem(path);

            // Check file size entitlement
            if (!_entitlementService.CanProcessFile(fileItem.FileSizeBytes, out var fileReason))
            {
                ErrorMessage = fileReason;
                continue;
            }

            Files.Add(fileItem);
        }

        if (SelectedFile == null && Files.Count > 0)
        {
            SelectedFile = Files[0];
        }

        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(FilesVisibility));
        OnPropertyChanged(nameof(CanMerge));

        // Load page counts asynchronously
        foreach (var file in Files.Where(f => f.PageCount == 0))
        {
            try
            {
                int count = await _pdfService.GetPageCountAsync(file.FilePath);
                file.PageCount = count;
                file.DetailText = $"{count} page{(count == 1 ? "" : "s")}";
            }
            catch
            {
                file.DetailText = "Unable to read page count";
            }
        }
    }

    [RelayCommand]
    private async Task PickFilesAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            if (App.MainWindowHandle != 0)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindowHandle);
            }

            var picked = await picker.PickMultipleFilesAsync();
            if (picked != null && picked.Count > 0)
            {
                await AddFilesAsync(picked.Select(f => f.Path));
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error picking PDF files", ex);
        }
    }

    [RelayCommand]
    private void MoveSelectedUp()
    {
        if (SelectedFile == null) return;
        int index = Files.IndexOf(SelectedFile);
        if (index > 0)
        {
            Files.Move(index, index - 1);
            OnPropertyChanged(nameof(CanMoveSelectedUp));
            OnPropertyChanged(nameof(CanMoveSelectedDown));
        }
    }

    [RelayCommand]
    private void MoveSelectedDown()
    {
        if (SelectedFile == null) return;
        int index = Files.IndexOf(SelectedFile);
        if (index >= 0 && index < Files.Count - 1)
        {
            Files.Move(index, index + 1);
            OnPropertyChanged(nameof(CanMoveSelectedUp));
            OnPropertyChanged(nameof(CanMoveSelectedDown));
        }
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        if (SelectedFile == null) return;
        int index = Files.IndexOf(SelectedFile);
        Files.Remove(SelectedFile);

        if (Files.Count > 0)
        {
            SelectedFile = Files[Math.Min(index, Files.Count - 1)];
        }
        else
        {
            SelectedFile = null;
        }

        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(FilesVisibility));
        OnPropertyChanged(nameof(CanMerge));
    }

    [RelayCommand]
    private void ClearFiles()
    {
        Files.Clear();
        SelectedFile = null;
        Result = null;
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(FilesVisibility));
        OnPropertyChanged(nameof(CanMerge));
    }

    [RelayCommand]
    private async Task ExecuteMergeAsync()
    {
        if (Files.Count < 2 || IsBusy) return;

        if (!_entitlementService.TryConsumeOperation())
        {
            ErrorMessage = "You have reached your free daily operation limit (5/day). Upgrade to Pro for unlimited operations.";
            return;
        }

        IsBusy = true;
        Result = null;
        ErrorMessage = null;
        ProgressPercent = 0;
        StatusText = "Preparing merge...";
        _cts = new CancellationTokenSource();

        try
        {
            var outputFolder = _fileService.GetDefaultOutputDirectory();
            var paths = Files.Select(f => f.FilePath).ToList();

            var progress = new Progress<OperationProgress>(p =>
            {
                ProgressPercent = p.Percentage;
                StatusText = p.StatusMessage;
            });

            var res = await _pdfService.MergePdfsAsync(paths, outputFolder, progress, _cts.Token);
            Result = res;

            if (!res.Success)
            {
                ErrorMessage = res.ErrorMessage;
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Merge canceled.";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Unexpected error during merge", ex);
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
