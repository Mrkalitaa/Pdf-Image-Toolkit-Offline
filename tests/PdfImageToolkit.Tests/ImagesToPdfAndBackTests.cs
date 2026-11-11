using ImageMagick;
using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PdfImageToolkit.Tests;

public class ImagesToPdfAndBackTests : IDisposable
{
    private readonly string _testDir;
    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;
    private readonly PdfService _pdfService;

    public ImagesToPdfAndBackTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ImagesToPdfTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        _loggingService = new LoggingService();
        var settings = new SettingsService { DefaultOutputFolder = _testDir };
        _fileService = new FileService(settings);
        _pdfService = new PdfService(_fileService, _loggingService);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Ignore
        }
    }

    private string CreateSampleImage(string fileName, int width, int height, string colorName)
    {
        var filePath = Path.Combine(_testDir, fileName);
        var settings = new MagickReadSettings
        {
            Width = (uint)width,
            Height = (uint)height
        };
        using var image = new MagickImage($"xc:{colorName}", settings);
        image.Write(filePath);
        return filePath;
    }

    [Fact]
    public async Task ImagesToPdf_ConvertsMultipleImages_ProducesValidPdf()
    {
        var img1 = CreateSampleImage("test1.png", 200, 200, "red");
        var img2 = CreateSampleImage("test2.jpg", 300, 150, "blue");

        var result = await _pdfService.ImagesToPdfAsync([img1, img2], PageSizePreset.A4, 20, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));

        int pageCount = await _pdfService.GetPageCountAsync(result.OutputFilePath);
        Assert.Equal(2, pageCount);
    }

    [Fact]
    public async Task PdfToImages_RendersPagesToPngImages()
    {
        var img1 = CreateSampleImage("source1.png", 150, 150, "green");
        var img2 = CreateSampleImage("source2.png", 150, 150, "yellow");

        var pdfResult = await _pdfService.ImagesToPdfAsync([img1, img2], PageSizePreset.FitImage, 0, _testDir);
        Assert.True(pdfResult.Success);
        Assert.NotNull(pdfResult.OutputFilePath);

        var imagesResult = await _pdfService.PdfToImagesAsync(pdfResult.OutputFilePath, null, "PNG", 96, _testDir);

        Assert.True(imagesResult.Success);
        Assert.NotNull(imagesResult.OutputFilePaths);
        Assert.Equal(2, imagesResult.OutputFilePaths.Count);

        foreach (var imgPath in imagesResult.OutputFilePaths)
        {
            Assert.True(File.Exists(imgPath));
            using var readBack = new MagickImage(imgPath);
            Assert.True(readBack.Width > 0);
            Assert.True(readBack.Height > 0);
        }
    }
}
