using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfImageToolkit.Core.Helpers;
using PdfImageToolkit.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PdfImageToolkit.Core.Services;

public class PdfService : IPdfService
{
    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;

    public PdfService(IFileService fileService, ILoggingService loggingService)
    {
        _fileService = fileService;
        _loggingService = loggingService;
    }

    public Task<int> GetPageCountAsync(string pdfPath, string? password = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var doc = string.IsNullOrEmpty(password)
                    ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)
                    : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Import);
                return doc.PageCount;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to read page count for: {pdfPath}", ex);
                throw;
            }
        }, cancellationToken);
    }

    public Task<bool> IsPasswordProtectedAsync(string pdfPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var doc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                return false;
            }
            catch (PdfReaderException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            catch
            {
                return false;
            }
        }, cancellationToken);
    }

    public Task<OperationResult> MergePdfsAsync(
        IReadOnlyList<string> pdfPaths,
        string outputFolder,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (pdfPaths == null || pdfPaths.Count == 0)
            {
                return OperationResult.Failed("No PDF files selected for merging.");
            }

            try
            {
                long originalTotalSize = 0;
                foreach (var p in pdfPaths)
                {
                    originalTotalSize += _fileService.GetFileSize(p);
                }

                using var outputDocument = new PdfDocument();
                int totalFiles = pdfPaths.Count;

                for (int fileIndex = 0; fileIndex < totalFiles; fileIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var path = pdfPaths[fileIndex];
                    var fileName = Path.GetFileName(path);
                    progress?.Report(new OperationProgress(
                        Percentage: (double)fileIndex / totalFiles * 100.0,
                        StatusMessage: $"Merging {fileName} ({fileIndex + 1}/{totalFiles})...",
                        CurrentItemIndex: fileIndex + 1,
                        TotalItems: totalFiles));

                    using var inputDocument = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                    int pageCount = inputDocument.PageCount;
                    for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        outputDocument.AddPage(inputDocument.Pages[pageIndex]);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(new OperationProgress(95, "Saving merged document..."));

                var firstFileName = Path.GetFileNameWithoutExtension(pdfPaths[0]);
                var baseName = pdfPaths.Count > 1 ? $"{firstFileName}_merged" : firstFileName;
                var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, "pdf", string.Empty);

                outputDocument.Save(outputPath);

                long outputSize = _fileService.GetFileSize(outputPath);
                progress?.Report(new OperationProgress(100, "Merge complete!", totalFiles, totalFiles));

                _loggingService.LogInfo($"Merged {totalFiles} PDFs into: {outputPath}");
                return OperationResult.Succeeded(outputPath, originalTotalSize, outputSize);
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogInfo("Merge operation canceled by user.");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Merge failed.", ex);
                return OperationResult.Failed($"Merge failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<OperationResult> SplitPdfAsync(
        string pdfPath,
        string pageRangeExpression,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(pdfPath);

                using var inputDocument = string.IsNullOrEmpty(password)
                    ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)
                    : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Import);

                int totalPages = inputDocument.PageCount;
                var selectedPages = PageRangeParser.Parse(pageRangeExpression, totalPages);

                if (selectedPages.Count == 0)
                {
                    return OperationResult.Failed("No pages matched the specified range expression.");
                }

                using var outputDocument = new PdfDocument();
                int processed = 0;
                foreach (var pageNum in selectedPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // 1-based pageNum to 0-based index
                    outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
                    processed++;
                    progress?.Report(new OperationProgress(
                        Percentage: (double)processed / selectedPages.Count * 90.0,
                        StatusMessage: $"Extracting page {pageNum} of {totalPages}...",
                        CurrentItemIndex: processed,
                        TotalItems: selectedPages.Count));
                }

                var baseName = Path.GetFileNameWithoutExtension(pdfPath);
                var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, "pdf", "split");

                outputDocument.Save(outputPath);
                long outputSize = _fileService.GetFileSize(outputPath);
                progress?.Report(new OperationProgress(100, "Split complete!", processed, selectedPages.Count));

                _loggingService.LogInfo($"Extracted {selectedPages.Count} pages from {pdfPath} to: {outputPath}");
                return OperationResult.Succeeded(outputPath, originalSize, outputSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Split by range failed.", ex);
                return OperationResult.Failed($"Split failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<OperationResult> SplitEveryPageAsync(
        string pdfPath,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(pdfPath);

                using var inputDocument = string.IsNullOrEmpty(password)
                    ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)
                    : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Import);

                int totalPages = inputDocument.PageCount;
                var baseName = Path.GetFileNameWithoutExtension(pdfPath);
                var createdPaths = new List<string>();

                for (int i = 0; i < totalPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var singlePageDoc = new PdfDocument();
                    singlePageDoc.AddPage(inputDocument.Pages[i]);

                    var pageNumber = i + 1;
                    var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, $"{baseName}_page_{pageNumber}", "pdf", string.Empty);
                    singlePageDoc.Save(outputPath);
                    createdPaths.Add(outputPath);

                    progress?.Report(new OperationProgress(
                        Percentage: (double)(i + 1) / totalPages * 100.0,
                        StatusMessage: $"Exporting page {pageNumber} of {totalPages}...",
                        CurrentItemIndex: pageNumber,
                        TotalItems: totalPages));
                }

                _loggingService.LogInfo($"Split {totalPages} individual pages from {pdfPath}");
                return OperationResult.Succeeded(createdPaths, originalSize, originalSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Split every page failed.", ex);
                return OperationResult.Failed($"Split every page failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<OperationResult> SplitEveryNPagesAsync(
        string pdfPath,
        int pagesPerSplit,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (pagesPerSplit <= 0)
            {
                return OperationResult.Failed("Pages per split must be greater than zero.");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(pdfPath);

                using var inputDocument = string.IsNullOrEmpty(password)
                    ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)
                    : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Import);

                int totalPages = inputDocument.PageCount;
                var baseName = Path.GetFileNameWithoutExtension(pdfPath);
                var createdPaths = new List<string>();
                int partNumber = 1;

                for (int start = 0; start < totalPages; start += pagesPerSplit)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int end = Math.Min(start + pagesPerSplit, totalPages);

                    using var partDoc = new PdfDocument();
                    for (int p = start; p < end; p++)
                    {
                        partDoc.AddPage(inputDocument.Pages[p]);
                    }

                    var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, $"{baseName}_part_{partNumber}", "pdf", string.Empty);
                    partDoc.Save(outputPath);
                    createdPaths.Add(outputPath);
                    partNumber++;

                    progress?.Report(new OperationProgress(
                        Percentage: (double)end / totalPages * 100.0,
                        StatusMessage: $"Exporting part {partNumber - 1} (pages {start + 1}-{end})...",
                        CurrentItemIndex: end,
                        TotalItems: totalPages));
                }

                _loggingService.LogInfo($"Split {totalPages} pages into {createdPaths.Count} parts from {pdfPath}");
                return OperationResult.Succeeded(createdPaths, originalSize, originalSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Split every N pages failed.", ex);
                return OperationResult.Failed($"Split failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<OperationResult> RotatePdfAsync(
        string pdfPath,
        int rotationAngleDegrees,
        string? pageRangeExpression,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (rotationAngleDegrees % 90 != 0)
            {
                return OperationResult.Failed("Rotation angle must be a multiple of 90 degrees (90, 180, 270).");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(pdfPath);

                using var document = string.IsNullOrEmpty(password)
                    ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify)
                    : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Modify);

                int totalPages = document.PageCount;
                var targetPages = string.IsNullOrWhiteSpace(pageRangeExpression)
                    ? Enumerable.Range(1, totalPages).ToList()
                    : PageRangeParser.Parse(pageRangeExpression, totalPages);

                int processed = 0;
                foreach (var pageNum in targetPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var page = document.Pages[pageNum - 1];
                    int currentRotate = page.Rotate;
                    int newRotate = (currentRotate + rotationAngleDegrees) % 360;
                    if (newRotate < 0) newRotate += 360;
                    page.Rotate = newRotate;

                    processed++;
                    progress?.Report(new OperationProgress(
                        Percentage: (double)processed / targetPages.Count * 90.0,
                        StatusMessage: $"Rotating page {pageNum} of {totalPages} by {rotationAngleDegrees}°...",
                        CurrentItemIndex: processed,
                        TotalItems: targetPages.Count));
                }

                var baseName = Path.GetFileNameWithoutExtension(pdfPath);
                var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, "pdf", "rotated");

                document.Save(outputPath);
                long outputSize = _fileService.GetFileSize(outputPath);
                progress?.Report(new OperationProgress(100, "Rotation complete!", processed, targetPages.Count));

                _loggingService.LogInfo($"Rotated {targetPages.Count} pages in {pdfPath} by {rotationAngleDegrees}° to: {outputPath}");
                return OperationResult.Succeeded(outputPath, originalSize, outputSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Rotation failed.", ex);
                return OperationResult.Failed($"Rotation failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<OperationResult> CompressPdfLightAsync(
        string pdfPath,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long originalSize = _fileService.GetFileSize(pdfPath);

                progress?.Report(new OperationProgress(10, "Opening document and optimizing streams..."));

                using var inputDocument = string.IsNullOrEmpty(password)
                    ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)
                    : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Import);

                using var outputDocument = new PdfDocument();
                outputDocument.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
                outputDocument.Options.CompressContentStreams = true;

                int pageCount = inputDocument.PageCount;
                for (int i = 0; i < pageCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    outputDocument.AddPage(inputDocument.Pages[i]);
                    progress?.Report(new OperationProgress(
                        Percentage: 10.0 + ((double)(i + 1) / pageCount * 70.0),
                        StatusMessage: $"Re-encoding page streams ({i + 1}/{pageCount})...",
                        CurrentItemIndex: i + 1,
                        TotalItems: pageCount));
                }

                // Strip document metadata for privacy and size reduction
                outputDocument.Info.Title = string.Empty;
                outputDocument.Info.Author = string.Empty;
                outputDocument.Info.Subject = string.Empty;
                outputDocument.Info.Keywords = string.Empty;
                outputDocument.Info.Creator = string.Empty;

                var tempPath = _fileService.CreateTempFilePath("pdf");
                outputDocument.Save(tempPath);

                long tempSize = _fileService.GetFileSize(tempPath);

                // If output is larger or equal, keep original and notify
                if (tempSize >= originalSize)
                {
                    try { File.Delete(tempPath); } catch { }
                    _loggingService.LogInfo($"Light compression did not reduce size for {pdfPath} ({tempSize} >= {originalSize}). Original retained.");
                    return new OperationResult(
                        Success: true,
                        OutputFilePath: pdfPath,
                        OriginalSizeBytes: originalSize,
                        CompressedSizeBytes: originalSize,
                        WarningMessage: "The original document is already heavily compressed. Preserved original file to prevent size increase.");
                }

                var baseName = Path.GetFileNameWithoutExtension(pdfPath);
                var finalPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, "pdf", "compressed_light");
                File.Move(tempPath, finalPath, true);

                progress?.Report(new OperationProgress(100, "Light compression complete!", pageCount, pageCount));
                _loggingService.LogInfo($"Light compression reduced {pdfPath} from {originalSize} to {tempSize} bytes.");
                return OperationResult.Succeeded(finalPath, originalSize, tempSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Light compression failed.", ex);
                return OperationResult.Failed($"Light compression failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<OperationResult> CompressPdfStrongAsync(
        string pdfPath,
        int targetDpi,
        int jpegQuality,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            long originalSize = _fileService.GetFileSize(pdfPath);

            var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(pdfPath);
            Windows.Data.Pdf.PdfDocument winPdfDoc;

            if (string.IsNullOrEmpty(password))
            {
                winPdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(storageFile);
            }
            else
            {
                winPdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(storageFile, password);
            }

            int totalPages = (int)winPdfDoc.PageCount;
            if (totalPages == 0)
            {
                return OperationResult.Failed("Document has 0 pages.");
            }

            using var outputDoc = new PdfDocument();
            outputDoc.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
            outputDoc.Options.CompressContentStreams = true;

            for (int i = 0; i < totalPages; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var page = winPdfDoc.GetPage((uint)i);

                double pointWidth = page.Size.Width;
                double pointHeight = page.Size.Height;

                var renderOptions = new Windows.Data.Pdf.PdfPageRenderOptions
                {
                    DestinationWidth = (uint)Math.Max(1, Math.Round(pointWidth * (targetDpi / 72.0))),
                    DestinationHeight = (uint)Math.Max(1, Math.Round(pointHeight * (targetDpi / 72.0)))
                };

                using var randomAccessStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(randomAccessStream, renderOptions);

                using var netStream = System.IO.WindowsRuntimeStreamExtensions.AsStreamForRead(randomAccessStream);
                using var magick = new ImageMagick.MagickImage(netStream)
                {
                    Format = ImageMagick.MagickFormat.Jpg,
                    Quality = (uint)Math.Clamp(jpegQuality, 30, 95)
                };

                using var jpgMemStream = new MemoryStream();
                magick.Write(jpgMemStream);
                jpgMemStream.Position = 0;

                using var xImage = PdfSharp.Drawing.XImage.FromStream(jpgMemStream);

                var newPage = outputDoc.AddPage();
                newPage.Width = PdfSharp.Drawing.XUnit.FromPoint(pointWidth);
                newPage.Height = PdfSharp.Drawing.XUnit.FromPoint(pointHeight);

                using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(newPage);
                gfx.DrawImage(xImage, 0, 0, pointWidth, pointHeight);

                progress?.Report(new OperationProgress(
                    Percentage: (double)(i + 1) / totalPages * 90.0,
                    StatusMessage: $"Rasterizing page {i + 1} of {totalPages} at {targetDpi} DPI...",
                    CurrentItemIndex: i + 1,
                    TotalItems: totalPages));
            }

            var tempPath = _fileService.CreateTempFilePath("pdf");
            outputDoc.Save(tempPath);
            long tempSize = _fileService.GetFileSize(tempPath);

            if (tempSize >= originalSize)
            {
                try { File.Delete(tempPath); } catch { }
                _loggingService.LogInfo($"Strong compression did not reduce size ({tempSize} >= {originalSize}). Original retained.");
                return new OperationResult(
                    Success: true,
                    OutputFilePath: pdfPath,
                    OriginalSizeBytes: originalSize,
                    CompressedSizeBytes: originalSize,
                    WarningMessage: "Strong compression resulted in a larger file than original. Original file was kept.");
            }

            var baseName = Path.GetFileNameWithoutExtension(pdfPath);
            var finalPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, "pdf", "compressed_strong");
            File.Move(tempPath, finalPath, true);

            progress?.Report(new OperationProgress(100, "Strong compression complete!", totalPages, totalPages));
            _loggingService.LogInfo($"Strong compression reduced {pdfPath} from {originalSize} to {tempSize} bytes.");
            return OperationResult.Succeeded(
                finalPath,
                originalSize,
                tempSize,
                warning: "Pages were rasterized into high-efficiency JPEG images. Text in this PDF is image-based and no longer selectable.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Strong compression failed.", ex);
            return OperationResult.Failed($"Strong compression failed: {ex.Message}");
        }
    }

    public async Task<OperationResult> ImagesToPdfAsync(
        IReadOnlyList<string> imagePaths,
        PageSizePreset pageSize,
        int marginPoints,
        string outputFolder,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (imagePaths == null || imagePaths.Count == 0)
            {
                return OperationResult.Failed("No images provided to convert to PDF.");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                long totalOriginalSize = 0;
                foreach (var p in imagePaths)
                {
                    totalOriginalSize += _fileService.GetFileSize(p);
                }

                using var outputDoc = new PdfDocument();
                int totalImages = imagePaths.Count;

                for (int i = 0; i < totalImages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var imagePath = imagePaths[i];
                    var imageName = Path.GetFileName(imagePath);

                    progress?.Report(new OperationProgress(
                        Percentage: (double)i / totalImages * 90.0,
                        StatusMessage: $"Processing image {i + 1} of {totalImages} ({imageName})...",
                        CurrentItemIndex: i + 1,
                        TotalItems: totalImages));

                    // Use Magick.NET to decode any image format (JPG, PNG, WebP, etc.) and get dimensions
                    using var magickImage = new ImageMagick.MagickImage(imagePath);
                    using var pngMemStream = new MemoryStream();
                    magickImage.Format = ImageMagick.MagickFormat.Png;
                    magickImage.Write(pngMemStream);
                    pngMemStream.Position = 0;

                    using var xImage = PdfSharp.Drawing.XImage.FromStream(pngMemStream);

                    double pageWidth;
                    double pageHeight;

                    switch (pageSize)
                    {
                        case PageSizePreset.A4:
                            pageWidth = 595.28;
                            pageHeight = 841.89;
                            break;
                        case PageSizePreset.Letter:
                            pageWidth = 612.0;
                            pageHeight = 792.0;
                            break;
                        case PageSizePreset.FitImage:
                        default:
                            pageWidth = xImage.PointWidth + (marginPoints * 2);
                            pageHeight = xImage.PointHeight + (marginPoints * 2);
                            break;
                    }

                    var page = outputDoc.AddPage();
                    page.Width = PdfSharp.Drawing.XUnit.FromPoint(pageWidth);
                    page.Height = PdfSharp.Drawing.XUnit.FromPoint(pageHeight);

                    using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);

                    double maxWidth = Math.Max(10, pageWidth - (marginPoints * 2));
                    double maxHeight = Math.Max(10, pageHeight - (marginPoints * 2));

                    double scale = Math.Min(maxWidth / xImage.PointWidth, maxHeight / xImage.PointHeight);
                    double drawWidth = xImage.PointWidth * scale;
                    double drawHeight = xImage.PointHeight * scale;

                    double drawX = (pageWidth - drawWidth) / 2.0;
                    double drawY = (pageHeight - drawHeight) / 2.0;

                    gfx.DrawImage(xImage, drawX, drawY, drawWidth, drawHeight);
                }

                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(new OperationProgress(95, "Saving PDF document..."));

                var firstImageName = Path.GetFileNameWithoutExtension(imagePaths[0]);
                var baseName = imagePaths.Count > 1 ? $"{firstImageName}_images" : firstImageName;
                var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, baseName, "pdf", string.Empty);

                outputDoc.Save(outputPath);
                long outputSize = _fileService.GetFileSize(outputPath);
                progress?.Report(new OperationProgress(100, "Images converted to PDF successfully!", totalImages, totalImages));

                _loggingService.LogInfo($"Converted {totalImages} images to PDF: {outputPath}");
                return OperationResult.Succeeded(outputPath, totalOriginalSize, outputSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("ImagesToPdf failed.", ex);
                return OperationResult.Failed($"Images to PDF failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<OperationResult> PdfToImagesAsync(
        string pdfPath,
        string? pageRangeExpression,
        string format,
        int dpi,
        string outputFolder,
        string? password = null,
        IProgress<OperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            long originalSize = _fileService.GetFileSize(pdfPath);

            var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(pdfPath);
            Windows.Data.Pdf.PdfDocument winPdfDoc;

            if (string.IsNullOrEmpty(password))
            {
                winPdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(storageFile);
            }
            else
            {
                winPdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(storageFile, password);
            }

            int totalPages = (int)winPdfDoc.PageCount;
            var targetPages = string.IsNullOrWhiteSpace(pageRangeExpression)
                ? Enumerable.Range(1, totalPages).ToList()
                : PageRangeParser.Parse(pageRangeExpression, totalPages);

            if (targetPages.Count == 0)
            {
                return OperationResult.Failed("No pages selected for image export.");
            }

            var baseName = Path.GetFileNameWithoutExtension(pdfPath);
            var extension = format.Equals("JPG", StringComparison.OrdinalIgnoreCase) ? "jpg" : "png";
            var createdPaths = new List<string>();
            int processed = 0;

            foreach (var pageNum in targetPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var page = winPdfDoc.GetPage((uint)(pageNum - 1));
                var options = new Windows.Data.Pdf.PdfPageRenderOptions
                {
                    DestinationWidth = (uint)Math.Max(1, Math.Round(page.Size.Width * (dpi / 72.0))),
                    DestinationHeight = (uint)Math.Max(1, Math.Round(page.Size.Height * (dpi / 72.0)))
                };

                using var randomAccessStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(randomAccessStream, options);

                using var netStream = System.IO.WindowsRuntimeStreamExtensions.AsStreamForRead(randomAccessStream);
                using var magick = new ImageMagick.MagickImage(netStream);

                if (extension.Equals("jpg", StringComparison.OrdinalIgnoreCase))
                {
                    magick.Format = ImageMagick.MagickFormat.Jpg;
                    magick.Quality = 90;
                }
                else
                {
                    magick.Format = ImageMagick.MagickFormat.Png;
                }

                var outputPath = _fileService.GetUniqueDestinationPath(outputFolder, $"{baseName}_page_{pageNum}", extension, string.Empty);
                magick.Write(outputPath);
                createdPaths.Add(outputPath);

                processed++;
                progress?.Report(new OperationProgress(
                    Percentage: (double)processed / targetPages.Count * 100.0,
                    StatusMessage: $"Exported page {pageNum} to {extension.ToUpperInvariant()} ({processed}/{targetPages.Count})...",
                    CurrentItemIndex: processed,
                    TotalItems: targetPages.Count));
            }

            _loggingService.LogInfo($"Rendered {createdPaths.Count} pages of {pdfPath} to {extension.ToUpperInvariant()} at {dpi} DPI.");
            return OperationResult.Succeeded(createdPaths, originalSize, createdPaths.Sum(p => _fileService.GetFileSize(p)));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("PdfToImages failed.", ex);
            return OperationResult.Failed($"PDF to Images failed: {ex.Message}");
        }
    }
}
