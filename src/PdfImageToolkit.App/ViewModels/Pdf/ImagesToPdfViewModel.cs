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

public partial class ImagesToPdfViewModel : ObservableObject
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif", ".tiff", ".tif", ".heic"
    };

    private readonly IPdfService _pdfService;
    private readonly IFileService _fileService;
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    private CancellationTokenSource? _cts;

    public ImagesToPdfViewModel(
        IPdfService pdfService,
        IFileService fileService,
        IEntitlementService entitlementService,
        ILoggingService loggingService)
    {
        _pdfService = pdfService;
        _fileService = fileService;
        _entitlementService = entitlementService;
        _loggingService = loggingService;
        Images = [];
    }

    public ObservableCollection<FileItem> Images { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedImage))]
    [NotifyPropertyChangedFor(nameof(CanMoveSelectedUp))]
    [NotifyPropertyChangedFor(nameof(CanMoveSelectedDown))]
    private FileItem? _selectedImage;

    [ObservableProperty]
    private int _pageSizeIndex = 0; // 0 = A4, 1 = Letter, 2 = Fit Image

    [ObservableProperty]
    private int _marginIndex = 1; // 0 = 0pt, 1 = 18pt, 2 = 36pt, 3 = 72pt

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConvert))]
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
    [NotifyPropertyChangedFor(nameof(PdfFilePath))]
    private OperationResult? _result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool HasImages => Images.Count > 0;
    public bool HasSelectedImage => SelectedImage != null;
    public bool CanMoveSelectedUp => SelectedImage != null && Images.IndexOf(SelectedImage) > 0;
    public bool CanMoveSelectedDown => SelectedImage != null && Images.IndexOf(SelectedImage) < Images.Count - 1;
    public bool CanConvert => Images.Count >= 1 && !IsBusy;
    public bool HasResult => Result != null && Result.Success;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
    public string PdfFilePath => Result?.OutputFilePath ?? string.Empty;

    public Visibility ImagesVisibility => HasImages ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ResultVisibility => HasResult ? Visibility.Visible : Visibility.Collapsed;

    public PageSizePreset GetPageSizePreset() => PageSizeIndex switch
    {
        1 => PageSizePreset.Letter,
        2 => PageSizePreset.FitImage,
        _ => PageSizePreset.A4
    };

    public int GetMarginPoints() => MarginIndex switch
    {
        0 => 0,
        2 => 36,
        3 => 72,
        _ => 18
    };

    public void AddImages(IEnumerable<string> paths)
    {
        ErrorMessage = null;
        var valid = paths.Where(p => SupportedExtensions.Contains(Path.GetExtension(p))).ToList();
        if (valid.Count == 0) return;

        // Check batch size
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

            item.DetailText = "Image";
            Images.Add(item);
        }

        if (SelectedImage == null && Images.Count > 0)
        {
            SelectedImage = Images[0];
        }

        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(ImagesVisibility));
        OnPropertyChanged(nameof(CanConvert));
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
    private void MoveSelectedUp()
    {
        if (SelectedImage == null) return;
        int idx = Images.IndexOf(SelectedImage);
        if (idx > 0)
        {
            Images.Move(idx, idx - 1);
            OnPropertyChanged(nameof(CanMoveSelectedUp));
            OnPropertyChanged(nameof(CanMoveSelectedDown));
        }
    }

    [RelayCommand]
    private void MoveSelectedDown()
    {
        if (SelectedImage == null) return;
        int idx = Images.IndexOf(SelectedImage);
        if (idx >= 0 && idx < Images.Count - 1)
        {
            Images.Move(idx, idx + 1);
            OnPropertyChanged(nameof(CanMoveSelectedUp));
            OnPropertyChanged(nameof(CanMoveSelectedDown));
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
        OnPropertyChanged(nameof(CanConvert));
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
        OnPropertyChanged(nameof(CanConvert));
    }

    [RelayCommand]
    private async Task ExecuteConvertAsync()
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
        StatusText = "Converting images to PDF...";
        _cts = new CancellationTokenSource();

        try
        {
            var outputFolder = _fileService.GetDefaultOutputDirectory();
            var paths = Images.Select(i => i.FilePath).ToList();

            var progress = new Progress<OperationProgress>(p =>
            {
                ProgressPercent = p.Percentage;
                StatusText = p.StatusMessage;
            });

            var res = await _pdfService.ImagesToPdfAsync(
                paths,
                GetPageSizePreset(),
                GetMarginPoints(),
                outputFolder,
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
            StatusText = "Conversion canceled.";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error in ImagesToPdf", ex);
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
        var targetPath = Result?.OutputFilePath;
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
