using ImageMagick;
using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PdfImageToolkit.Tests;

public class ImageToolsTests : IDisposable
{
    private readonly string _testDir;
    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;
    private readonly ImageService _imageService;

    public ImageToolsTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ImageToolsTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        _loggingService = new LoggingService();
        var settings = new SettingsService { DefaultOutputFolder = _testDir };
        _fileService = new FileService(settings);
        _imageService = new ImageService(_fileService, _loggingService);
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

    private string CreateTestImage(string name, MagickFormat format, int width = 200, int height = 200)
    {
        var path = Path.Combine(_testDir, name);
        var settings = new MagickReadSettings { Width = (uint)width, Height = (uint)height };
        using var img = new MagickImage("plasma:fractal", settings)
        {
            Format = format
        };
        img.Write(path);
        return path;
    }

    [Theory]
    [InlineData("PNG", "JPG")]
    [InlineData("JPG", "WEBP")]
    [InlineData("WEBP", "PNG")]
    public async Task Convert_BetweenAllFormats_Succeeds(string sourceFmt, string targetFmt)
    {
        var srcFormat = Enum.Parse<MagickFormat>(sourceFmt, true);
        var srcPath = CreateTestImage($"sample.{sourceFmt.ToLowerInvariant()}", srcFormat);

        var result = await _imageService.ConvertImageAsync(srcPath, targetFmt, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));

        using var converted = new MagickImage(result.OutputFilePath);
        if (targetFmt is "JPG" or "JPEG")
        {
            Assert.True(converted.Format is MagickFormat.Jpg or MagickFormat.Jpeg);
        }
        else
        {
            var expectedFormat = Enum.Parse<MagickFormat>(targetFmt, true);
            Assert.Equal(expectedFormat, converted.Format);
        }
    }

    [Fact]
    public async Task Resize_ByPercent_UpdatesDimensionsAccurately()
    {
        var src = CreateTestImage("resize_percent.png", MagickFormat.Png, 400, 200);

        var result = await _imageService.ResizeImageByPercentAsync(src, 50, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);

        using var resized = new MagickImage(result.OutputFilePath);
        Assert.Equal(200u, resized.Width);
        Assert.Equal(100u, resized.Height);
    }

    [Fact]
    public async Task Resize_TargetDimensionsWithAspectLock_PreservesAspect()
    {
        var src = CreateTestImage("resize_aspect.jpg", MagickFormat.Jpg, 1920, 1080);

        var result = await _imageService.ResizeImageAsync(src, 1280, 720, maintainAspectRatio: true, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);

        using var resized = new MagickImage(result.OutputFilePath);
        Assert.Equal(1280u, resized.Width);
        Assert.Equal(720u, resized.Height);
    }

    [Fact]
    public async Task CompressImage_NeverReturnsLargerFile()
    {
        var src = CreateTestImage("compress_test.jpg", MagickFormat.Jpg, 600, 600);
        long originalSize = new FileInfo(src).Length;

        var result = await _imageService.CompressImageAsync(src, quality: 40, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);

        long outputSize = new FileInfo(result.OutputFilePath).Length;
        Assert.True(outputSize <= originalSize);
    }

    [Fact]
    public async Task Resize_PresetSquare1080_Succeeds()
    {
        var src = CreateTestImage("square_preset.png", MagickFormat.Png, 2000, 2000);

        var result = await _imageService.ResizeImageAsync(src, 1080, 1080, maintainAspectRatio: true, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);

        using var resized = new MagickImage(result.OutputFilePath);
        Assert.Equal(1080u, resized.Width);
        Assert.Equal(1080u, resized.Height);
    }

    [Fact]
    public async Task ConvertImage_StripMetadata_ProducesValidImage()
    {
        var src = CreateTestImage("metadata_test.jpg", MagickFormat.Jpg, 300, 300);

        var result = await _imageService.ConvertImageAsync(src, "PNG", _testDir, stripMetadata: true);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));

        using var img = new MagickImage(result.OutputFilePath);
        Assert.Equal(MagickFormat.Png, img.Format);
    }
}
