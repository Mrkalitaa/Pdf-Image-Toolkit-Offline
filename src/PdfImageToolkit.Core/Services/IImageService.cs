using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PdfImageToolkit.Core.Models;

namespace PdfImageToolkit.Core.Services;

public interface IImageService
{
    Task<OperationResult> CompressImageAsync(
        string imagePath, 
        int quality, 
        string outputFolder, 
        bool stripMetadata = false, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> ResizeImageAsync(
        string imagePath, 
        int targetWidth, 
        int targetHeight, 
        bool maintainAspectRatio, 
        string outputFolder, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> ResizeImageByPercentAsync(
        string imagePath, 
        double scalePercent, 
        string outputFolder, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> ConvertImageAsync(
        string imagePath, 
        string targetFormat, // "JPG", "PNG", "WEBP"
        string outputFolder, 
        bool stripMetadata = false, 
        IProgress<OperationProgress>? progress = null, 
        CancellationToken cancellationToken = default);
}
