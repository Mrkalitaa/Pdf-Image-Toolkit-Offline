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

public partial class ImageCompressViewModel : ObservableObject
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif", ".bmp"
    };

    private readonly IImageService _imageService;
    private readonly IFileService _fileService;
    private readonly IEntitlementService _entitlementService;
    private readonly ISettingsService _settingsService;
    private readonly ILoggingService _loggingService;

    private CancellationTokenSource? _cts;

    public ImageCompressViewModel(
        IImageService imageService,
        IFileService fileService,
        IEntitlementService entitlementService,
        ISettingsService settingsService,
        ILoggingService loggingService)
    {
        _imageService = imageService;
        _fileService = fileService;
        _entitlementService = entitlementService;
        _settingsService = settingsService;
        _loggingService = loggingService;

        _stripMetadata = _settingsService.StripMetadataDefault;
        Images = [];
    }

    public ObservableCollection<FileItem> Images { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedImage))]
    private FileItem? _selectedImage;

    [ObservableProperty]
    private double _quality = 75;

    [ObservableProperty]
    private bool _stripMetadata;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCompress))]
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
    public bool CanCompress => Images.Count > 0 && !IsBusy;
    public bool HasResult => Result != null && Result.Success;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    public Visibility ImagesVisibility => HasImages ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ResultVisibility => HasResult ? Visibility.Visible : Visibility.Collapsed;

    public string OutputSummaryText
    {
        get
        {
            if (Result == null) return string.Empty;
            if (Result.OutputFilePath != null)
            {
                var orig = FileItem.FormatBytes(Result.OriginalSizeBytes);
                var comp = FileItem.FormatBytes(Result.CompressedSizeBytes);
                return $"{Result.OutputFilePath} ({orig} → {comp}, {Result.SavingsPercentage}% saved)";
            }
            if (Result.OutputFilePaths != null)
            {
                var orig = FileItem.FormatBytes(Result.OriginalSizeBytes);
                var comp = FileItem.FormatBytes(Result.CompressedSizeBytes);
                return $"Processed {Result.OutputFilePaths.Count} images ({orig} → {comp}, {Result.SavingsPercentage}% saved)";
            }
            return "Compression complete.";
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
        OnPropertyChanged(nameof(CanCompress));
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
            _loggingService.LogError("Error picking images", ex);
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
        OnPropertyChanged(nameof(CanCompress));
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
        OnPropertyChanged(nameof(CanCompress));
    }

    [RelayCommand]
    private async Task ExecuteCompressAsync()
    {
        if (Images.Count == 0 || IsBusy) return;

        if (!_entitlementService.TryConsumeOperation())
        {
            ErrorMessage = "You have reached your free daily operation limit (5/day). Upgrade to Pro for unlimited operations.";
            return;
        }

        IsBusy = true;
        Result = null;
        ErrorMessage = null;
        ProgressPercent = 0;
        StatusText = "Compressing images...";
        _cts = new CancellationTokenSource();

        try
        {
            var outputFolder = _fileService.GetDefaultOutputDirectory();
            var createdFiles = new List<string>();
            long totalOriginal = 0;
            long totalCompressed = 0;

            int total = Images.Count;
            for (int i = 0; i < total; i++)
            {
                _cts.Token.ThrowIfCancellationRequested();
                var item = Images[i];

                ProgressPercent = (double)i / total * 100.0;
                StatusText = $"Compressing {item.FileName} ({i + 1}/{total})...";

                var res = await _imageService.CompressImageAsync(
                    item.FilePath,
                    (int)Quality,
                    outputFolder,
                    StripMetadata,
                    null,
                    _cts.Token);

                if (res.Success && res.OutputFilePath != null)
                {
                    createdFiles.Add(res.OutputFilePath);
                    totalOriginal += res.OriginalSizeBytes;
                    totalCompressed += res.CompressedSizeBytes;
                }
            }

            ProgressPercent = 100;
            StatusText = "Compression complete!";

            Result = createdFiles.Count == 1
                ? OperationResult.Succeeded(createdFiles[0], totalOriginal, totalCompressed)
                : OperationResult.Succeeded(createdFiles, totalOriginal, totalCompressed);
        }
        catch (OperationCanceledException)
        {
            StatusText = "Compression canceled.";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error compressing images", ex);
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
