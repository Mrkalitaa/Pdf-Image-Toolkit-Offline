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

namespace PdfImageToolkit.ViewModels.Image;

public partial class ImageResizeViewModel : ObservableObject
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif", ".bmp"
    };

    private readonly IImageService _imageService;
    private readonly IFileService _fileService;
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    private CancellationTokenSource? _cts;

    public ImageResizeViewModel(
        IImageService imageService,
        IFileService fileService,
        IEntitlementService entitlementService,
        ILoggingService loggingService)
    {
        _imageService = imageService;
        _fileService = fileService;
        _entitlementService = entitlementService;
        _loggingService = loggingService;

        Images = [];
    }

    public ObservableCollection<FileItem> Images { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedImage))]
    private FileItem? _selectedImage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPercentageMode))]
    [NotifyPropertyChangedFor(nameof(IsDimensionsMode))]
    [NotifyPropertyChangedFor(nameof(PercentageVisibility))]
    [NotifyPropertyChangedFor(nameof(DimensionsVisibility))]
    private int _resizeModeIndex = 0; // 0 = Percentage, 1 = Dimensions

    [ObservableProperty]
    private double _scalePercent = 50.0;

    [ObservableProperty]
    private int _targetWidth = 1920;

    [ObservableProperty]
    private int _targetHeight = 1080;

    [ObservableProperty]
    private bool _maintainAspectRatio = true;

    [ObservableProperty]
    private int _presetIndex = 0; // 0 = None / Custom, 1 = 1080x1080, 2 = 1280x720, 3 = 1920x1080

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanResize))]
    [NotifyPropertyChangedFor(nameof(HasImages))]
    [NotifyPropertyChangedFor(nameof(ImagesVisibility))]
    [NotifyPropertyChangedFor(nameof(BusyVisibility))]
    private bool _isBusy;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(ResultVisibility))]
    [NotifyPropertyChangedFor(nameof(OutputSummaryText))]
    private OperationResult? _result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool HasImages => Images.Count > 0;
    public bool HasSelectedImage => SelectedImage != null;
    public bool CanResize => Images.Count > 0 && !IsBusy;
    public bool HasResult => Result != null && Result.Success;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsPercentageMode => ResizeModeIndex == 0;
    public bool IsDimensionsMode => ResizeModeIndex == 1;

    public Visibility ImagesVisibility => HasImages ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ResultVisibility => HasResult ? Visibility.Visible : Visibility.Collapsed;
    public Visibility PercentageVisibility => IsPercentageMode ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DimensionsVisibility => IsDimensionsMode ? Visibility.Visible : Visibility.Collapsed;

    public string OutputSummaryText
    {
        get
        {
            if (Result == null) return string.Empty;
            if (Result.OutputFilePath != null)
            {
                var orig = FileItem.FormatBytes(Result.OriginalSizeBytes);
                var comp = FileItem.FormatBytes(Result.CompressedSizeBytes);
                return $"{Result.OutputFilePath} ({orig} → {comp})";
            }
            if (Result.OutputFilePaths != null)
            {
                var orig = FileItem.FormatBytes(Result.OriginalSizeBytes);
                var comp = FileItem.FormatBytes(Result.CompressedSizeBytes);
                return $"Resized {Result.OutputFilePaths.Count} images ({orig} → {comp})";
            }
            return "Resize complete.";
        }
    }

    partial void OnPresetIndexChanged(int value)
    {
        switch (value)
        {
            case 1: // 1080 x 1080
                TargetWidth = 1080;
                TargetHeight = 1080;
                break;
            case 2: // 1280 x 720
                TargetWidth = 1280;
                TargetHeight = 720;
                break;
            case 3: // 1920 x 1080
                TargetWidth = 1920;
                TargetHeight = 1080;
                break;
        }
    }

    public void AddImages(IEnumerable<string> paths)
    {
        ErrorMessage = null;
        var valid = paths.Where(p => SupportedExtensions.Contains(Path.GetExtension(p))).ToList();
        if (valid.Count == 0) return;

        int projected = Images.Count + valid.Count;
        if (!_entitlementService.CanProcessBatch(projected, out var batchReason))
        {
            ErrorMessage = batchReason;
            return;
        }

        foreach (var path in valid)
        {
            var item = new FileItem(path);
            if (!_entitlementService.CanProcessFile(item.FileSizeBytes, out var fileReason))
            {
                ErrorMessage = fileReason;
                continue;
            }

            Images.Add(item);
        }

        if (SelectedImage == null && Images.Count > 0)
        {
            SelectedImage = Images[0];
        }

        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(ImagesVisibility));
        OnPropertyChanged(nameof(CanResize));
    }

    [RelayCommand]
    private async Task PickImagesAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            foreach (var ext in SupportedExtensions)
            {
                picker.FileTypeFilter.Add(ext);
            }

            if (App.MainWindowHandle != 0)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindowHandle);
            }

            var picked = await picker.PickMultipleFilesAsync();
            if (picked != null && picked.Count > 0)
            {
                AddImages(picked.Select(f => f.Path));
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error picking images for resize", ex);
        }
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        if (SelectedImage == null) return;
        int idx = Images.IndexOf(SelectedImage);
        Images.Remove(SelectedImage);

        if (Images.Count > 0)
        {
            SelectedImage = Images[Math.Min(idx, Images.Count - 1)];
        }
        else
        {
            SelectedImage = null;
        }

        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(ImagesVisibility));
        OnPropertyChanged(nameof(CanResize));
    }

    [RelayCommand]
    private void ClearImages()
    {
        Images.Clear();
        SelectedImage = null;
        Result = null;
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(ImagesVisibility));
        OnPropertyChanged(nameof(CanResize));
    }

    [RelayCommand]
    private async Task ExecuteResizeAsync()
    {
        if (Images.Count == 0 || IsBusy) return;

        if (IsDimensionsMode && (TargetWidth <= 0 || TargetHeight <= 0))
        {
            ErrorMessage = "Target width and height must be greater than zero.";
            return;
        }

        if (IsPercentageMode && (ScalePercent <= 0 || ScalePercent > 1000))
        {
            ErrorMessage = "Scale percentage must be between 1% and 1000%.";
            return;
        }

        if (!_entitlementService.TryConsumeOperation())
        {
            ErrorMessage = "You have reached your free daily operation limit (5/day). Upgrade to Pro for unlimited operations.";
            return;
        }

        IsBusy = true;
        Result = null;
        ErrorMessage = null;
        ProgressPercent = 0;
        StatusText = "Resizing images...";
        _cts = new CancellationTokenSource();

        try
        {
            var outputFolder = _fileService.GetDefaultOutputDirectory();
            var createdFiles = new List<string>();
            long totalOriginal = 0;
            long totalResized = 0;

            int total = Images.Count;
            for (int i = 0; i < total; i++)
            {
                _cts.Token.ThrowIfCancellationRequested();
                var item = Images[i];

                ProgressPercent = (double)i / total * 100.0;
                StatusText = $"Resizing {item.FileName} ({i + 1}/{total})...";

                OperationResult res;
                if (IsPercentageMode)
                {
                    res = await _imageService.ResizeImageByPercentAsync(
                        item.FilePath,
                        ScalePercent,
                        outputFolder,
                        null,
                        _cts.Token);
                }
                else
                {
                    res = await _imageService.ResizeImageAsync(
                        item.FilePath,
                        TargetWidth,
                        TargetHeight,
                        MaintainAspectRatio,
                        outputFolder,
                        null,
                        _cts.Token);
                }

                if (res.Success && res.OutputFilePath != null)
                {
                    createdFiles.Add(res.OutputFilePath);
                    totalOriginal += res.OriginalSizeBytes;
                    totalResized += res.CompressedSizeBytes;
                }
            }

            ProgressPercent = 100;
            StatusText = "Resize complete!";

            Result = createdFiles.Count == 1
                ? OperationResult.Succeeded(createdFiles[0], totalOriginal, totalResized)
                : OperationResult.Succeeded(createdFiles, totalOriginal, totalResized);
        }
        catch (OperationCanceledException)
        {
            StatusText = "Resize canceled.";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error resizing images", ex);
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
        var target = Result?.OutputFilePath ?? Result?.OutputFilePaths?.FirstOrDefault();
        if (target != null && File.Exists(target))
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(target);
            await Launcher.LaunchFileAsync(file);
        }
    }

    [RelayCommand]
    private async Task OpenResultFolderAsync()
    {
        var target = Result?.OutputFilePath ?? Result?.OutputFilePaths?.FirstOrDefault();
        if (target != null && File.Exists(target))
        {
            var folderPath = Path.GetDirectoryName(target);
            if (folderPath != null && Directory.Exists(folderPath))
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                await Launcher.LaunchFolderAsync(folder);
            }
        }
    }
}
