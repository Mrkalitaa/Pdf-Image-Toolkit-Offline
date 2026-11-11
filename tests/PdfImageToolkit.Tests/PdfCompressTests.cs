using ImageMagick;
using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PdfImageToolkit.Tests;

public class PdfCompressTests : IDisposable
{
    private readonly string _testDir;
    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;
    private readonly PdfService _pdfService;

    public PdfCompressTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "PdfCompressTests_" + Guid.NewGuid().ToString("N"));
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

    private async Task<string> CreateTestPdfWithLargeImageAsync(string fileName)
    {
        var imgPath = Path.Combine(_testDir, "large_sample.png");
        var readSettings = new MagickReadSettings { Width = 800, Height = 800 };
        using (var img = new MagickImage("plasma:fractal", readSettings))
        {
            img.Write(imgPath);
        }

        var res = await _pdfService.ImagesToPdfAsync([imgPath], PageSizePreset.A4, 20, _testDir);
        return res.OutputFilePath!;
    }

    [Fact]
    public async Task CompressPdf_NeverReturnsLargerFile_LightMode()
    {
        var pdfPath = await CreateTestPdfWithLargeImageAsync("test_light.pdf");
        long originalSize = new FileInfo(pdfPath).Length;

        var result = await _pdfService.CompressPdfLightAsync(pdfPath, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);

        long outputSize = new FileInfo(result.OutputFilePath).Length;
        Assert.True(outputSize <= originalSize, $"Compressed file ({outputSize} bytes) was unexpectedly larger than original ({originalSize} bytes).");
    }

    [Fact]
    public async Task CompressPdf_NeverReturnsLargerFile_StrongMode()
    {
        var pdfPath = await CreateTestPdfWithLargeImageAsync("test_strong.pdf");
        long originalSize = new FileInfo(pdfPath).Length;

        var result = await _pdfService.CompressPdfStrongAsync(pdfPath, targetDpi: 96, jpegQuality: 60, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);

        long outputSize = new FileInfo(result.OutputFilePath).Length;
        Assert.True(outputSize <= originalSize, $"Strong compressed file ({outputSize} bytes) was unexpectedly larger than original ({originalSize} bytes).");
        Assert.True(result.SavingsPercentage >= 0.0);
    }
}
