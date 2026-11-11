using ImageMagick;
using PdfImageToolkit.Core.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace PdfImageToolkit.Core.Services;

public class ImageService : IImageService
{
    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;

    public ImageService(IFileService fileService, ILoggingService loggingService)
    {
        _fileService = fileService;
        _loggingService = loggingService;
    }

    private async Task<MagickImage> LoadImageWithWicFallbackAsync(string imagePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return new MagickImage(imagePath);
        }
        catch (Exception ex)
        {
            var ext = Path.GetExtension(imagePath).ToLowerInvariant();
            if (ext is ".heic" or ".heif")
            {
                _loggingService.LogWarning($"Magick.NET HEIC decode failed. Attempting Windows Imaging Component (WIC) fallback: {ex.Message}");
                try
                {
                    // WIC Fallback via Windows.Graphics.Imaging.BitmapDecoder
                    var file = await StorageFile.GetFileFromPathAsync(imagePath);
                    using var stream = await file.OpenReadAsync();
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var pixelData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        new BitmapTransform(),
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.ColorManageToSRgb);

                    var bytes = pixelData.DetachPixelData();
                    var readSettings = new PixelReadSettings(
                        (uint)decoder.PixelWidth,
                        (uint)decoder.PixelHeight,
                        StorageType.Char,
                        PixelMapping.BGRA);

                    var img = new MagickImage();
                    img.ReadPixels(bytes, readSettings);
                    return img;
                }
                catch (Exception wicEx)
                {
                    _loggingService.LogError("WIC HEIC fallback also failed. The Windows HEIF Image Extension may need to be installed from the Microsoft Store.", wicEx);
                    throw new InvalidOperationException("Could not read HEIC file. Please ensure the free 'HEIF Image Extensions' is installed from the Microsoft Store.", wicEx);
                }
            }

            throw;
        }
    }

    private void ApplyStripMetadata(MagickImage image, bool strip)
    {
        if (!strip) return;
        image.Strip();
        image.RemoveProfile("exif");
        image.RemoveProfile("iptc");
        image.RemoveProfile("xmp");
    }

    public async Task<OperationResult> CompressImageAsync(
        string imagePath,
        int quality,
        string outputFolder,
        bool stripMetadata = false,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(imagePath);

                progress?.Report(new OperationProgress(20, "Loading image for compression..."));
                using var image = await LoadImageWithWicFallbackAsync(imagePath, cancellationToken);

                ApplyStripMetadata(image, stripMetadata);

                var ext = Path.GetExtension(imagePath).ToLowerInvariant();
                if (ext is ".jpg" or ".jpeg")
                {
                    image.Format = MagickFormat.Jpg;
                    image.Quality = (uint)Math.Clamp(quality, 10, 100);
                }
                else if (ext is ".webp")
                {
                    image.Format = MagickFormat.WebP;
                    image.Quality = (uint)Math.Clamp(quality, 10, 100);
                }
                else
                {
                    // PNG or others
                    image.Format = MagickFormat.Png;
                    image.Quality = (uint)Math.Clamp(quality, 10, 100);
                }

                progress?.Report(new OperationProgress(70, "Encoding compressed image..."));

                var tempPath = _fileService.CreateTempFilePath(ext);
                image.Write(tempPath);
                long tempSize = _fileService.GetFileSize(tempPath);

                if (tempSize >= originalSize)
                {
                    try { File.Delete(tempPath); } catch { }
                    _loggingService.LogInfo($"Image compression did not reduce size ({tempSize} >= {originalSize}). Original retained.");
                    return new OperationResult(
                        Success: true,
                        OutputFilePath: imagePath,
                        OriginalSizeBytes: originalSize,
                        CompressedSizeBytes: originalSize,
                        WarningMessage: "The image is already compressed at or above the target quality. Original file was kept.");
                }

                var baseName = Path.GetFileNameWithoutExtension(imagePath);
                var finalPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, ext, "compressed");
                File.Move(tempPath, finalPath, true);

                progress?.Report(new OperationProgress(100, "Compression complete!", 1, 1));
                _loggingService.LogInfo($"Compressed image {imagePath} from {originalSize} to {tempSize} bytes.");
                return OperationResult.Succeeded(finalPath, originalSize, tempSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Compress image failed: {imagePath}", ex);
                return OperationResult.Failed($"Image compression failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<OperationResult> ResizeImageAsync(
        string imagePath,
        int targetWidth,
        int targetHeight,
        bool maintainAspectRatio,
        string outputFolder,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(imagePath);

                progress?.Report(new OperationProgress(20, "Loading image for resize..."));
                using var image = await LoadImageWithWicFallbackAsync(imagePath, cancellationToken);

                var geometry = new MagickGeometry((uint)Math.Max(1, targetWidth), (uint)Math.Max(1, targetHeight))
                {
                    IgnoreAspectRatio = !maintainAspectRatio
                };

                progress?.Report(new OperationProgress(60, "Resizing image dimensions..."));
                image.Resize(geometry);

                var ext = Path.GetExtension(imagePath);
                var baseName = Path.GetFileNameWithoutExtension(imagePath);
                var finalPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, ext, $"{image.Width}x{image.Height}");

                image.Write(finalPath);
                long newSize = _fileService.GetFileSize(finalPath);

                progress?.Report(new OperationProgress(100, "Resize complete!", 1, 1));
                _loggingService.LogInfo($"Resized image {imagePath} to {image.Width}x{image.Height}: {finalPath}");
                return OperationResult.Succeeded(finalPath, originalSize, newSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Resize image failed: {imagePath}", ex);
                return OperationResult.Failed($"Image resize failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<OperationResult> ResizeImageByPercentAsync(
        string imagePath,
        double scalePercent,
        string outputFolder,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(imagePath);

                progress?.Report(new OperationProgress(20, "Loading image for percentage scaling..."));
                using var image = await LoadImageWithWicFallbackAsync(imagePath, cancellationToken);

                double factor = Math.Clamp(scalePercent, 1.0, 500.0) / 100.0;
                uint newW = (uint)Math.Max(1, Math.Round(image.Width * factor));
                uint newH = (uint)Math.Max(1, Math.Round(image.Height * factor));

                progress?.Report(new OperationProgress(60, $"Rescaling to {newW}x{newH} ({scalePercent}%)..."));
                image.Resize(newW, newH);

                var ext = Path.GetExtension(imagePath);
                var baseName = Path.GetFileNameWithoutExtension(imagePath);
                var finalPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, ext, $"scale_{(int)scalePercent}pct");

                image.Write(finalPath);
                long newSize = _fileService.GetFileSize(finalPath);

                progress?.Report(new OperationProgress(100, "Scaling complete!", 1, 1));
                _loggingService.LogInfo($"Scaled image {imagePath} by {scalePercent}% to {finalPath}");
                return OperationResult.Succeeded(finalPath, originalSize, newSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Scale image failed: {imagePath}", ex);
                return OperationResult.Failed($"Image scale failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<OperationResult> ConvertImageAsync(
        string imagePath,
        string targetFormat,
        string outputFolder,
        bool stripMetadata = false,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(imagePath);

                progress?.Report(new OperationProgress(20, "Loading image for format conversion..."));
                using var image = await LoadImageWithWicFallbackAsync(imagePath, cancellationToken);

                ApplyStripMetadata(image, stripMetadata);

                var normalized = targetFormat.ToUpperInvariant();
                string targetExt;

                switch (normalized)
                {
                    case "PNG":
                        image.Format = MagickFormat.Png;
                        targetExt = "png";
                        break;
                    case "WEBP":
                        image.Format = MagickFormat.WebP;
                        image.Quality = 90;
                        targetExt = "webp";
                        break;
                    case "JPG":
                    case "JPEG":
                    default:
                        image.Format = MagickFormat.Jpg;
                        image.Quality = 90;
                        targetExt = "jpg";
                        break;
                }

                progress?.Report(new OperationProgress(70, $"Encoding to {normalized}..."));

                var baseName = Path.GetFileNameWithoutExtension(imagePath);
                var finalPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, targetExt, string.Empty);

                image.Write(finalPath);
                long newSize = _fileService.GetFileSize(finalPath);

                progress?.Report(new OperationProgress(100, "Conversion complete!", 1, 1));
                _loggingService.LogInfo($"Converted image {imagePath} to {finalPath}");
                return OperationResult.Succeeded(finalPath, originalSize, newSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Convert image failed: {imagePath}", ex);
                return OperationResult.Failed($"Image conversion failed: {ex.Message}");
            }
        }, cancellationToken);
    }
}
