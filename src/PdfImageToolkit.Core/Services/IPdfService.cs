using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PdfImageToolkit.Core.Models;

namespace PdfImageToolkit.Core.Services;

public enum PageSizePreset
{
    A4,
    Letter,
    FitImage
}

public interface IPdfService
{
    Task<int> GetPageCountAsync(string pdfPath, string? password = null, CancellationToken cancellationToken = default);
    Task<bool> IsPasswordProtectedAsync(string pdfPath, CancellationToken cancellationToken = default);
    
    Task<OperationResult> MergePdfsAsync(
        IReadOnlyList<string> pdfPaths, 
        string outputFolder, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> SplitPdfAsync(
        string pdfPath, 
        string pageRangeExpression, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> SplitEveryPageAsync(
        string pdfPath, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> SplitEveryNPagesAsync(
        string pdfPath, 
        int pagesPerSplit, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> RotatePdfAsync(
        string pdfPath, 
        int rotationAngleDegrees, 
        string? pageRangeExpression, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> CompressPdfLightAsync(
        string pdfPath, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> CompressPdfStrongAsync(
        string pdfPath, 
        int targetDpi, 
        int jpegQuality, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> ImagesToPdfAsync(
        IReadOnlyList<string> imagePaths, 
        PageSizePreset pageSize, 
        int marginPoints, 
        string outputFolder, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> PdfToImagesAsync(
        string pdfPath, 
        string? pageRangeExpression, 
        string format, // "PNG" or "JPG"
        int dpi, 
        string outputFolder, 
        string? password = null, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);
}
