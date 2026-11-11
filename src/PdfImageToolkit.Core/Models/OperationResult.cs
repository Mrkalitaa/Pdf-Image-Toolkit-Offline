using System;
using System.Collections.Generic;

namespace PdfImageToolkit.Core.Models;

public record OperationResult(
    bool Success,
    string? OutputFilePath = null,
    IReadOnlyList<string>? OutputFilePaths = null,
    string? ErrorMessage = null,
    long OriginalSizeBytes = 0,
    long CompressedSizeBytes = 0,
    string? WarningMessage = null)
{
    public static OperationResult Succeeded(string path, long originalSize = 0, long compressedSize = 0, string? warning = null)
        => new(true, OutputFilePath: path, OriginalSizeBytes: originalSize, CompressedSizeBytes: compressedSize, WarningMessage: warning);

    public static OperationResult Succeeded(IReadOnlyList<string> paths, long originalSize = 0, long compressedSize = 0, string? warning = null)
        => new(true, OutputFilePaths: paths, OriginalSizeBytes: originalSize, CompressedSizeBytes: compressedSize, WarningMessage: warning);

    public static OperationResult Failed(string error)
        => new(false, ErrorMessage: error);

    public double SavingsPercentage =>
        OriginalSizeBytes > 0 && CompressedSizeBytes < OriginalSizeBytes
            ? Math.Round((1.0 - (double)CompressedSizeBytes / OriginalSizeBytes) * 100.0, 1)
            : 0.0;
}

public record OperationProgress(
    double Percentage,
    string StatusMessage,
    int CurrentItemIndex = 0,
    int TotalItems = 0);
