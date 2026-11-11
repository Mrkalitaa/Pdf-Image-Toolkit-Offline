using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using Xunit;

namespace PdfImageToolkit.Tests;

public class FileServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly ISettingsService _settingsService;
    private readonly IFileService _fileService;

    public FileServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "PdfImageToolkit_FileTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        _settingsService = new SettingsService { DefaultOutputFolder = _testDir };
        _fileService = new FileService(_settingsService);
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

    [Fact]
    public void GetUniqueDestinationPath_NeverOverwritesOriginal_GeneratesIndexedNames()
    {
        var basePath = Path.Combine(_testDir, "sample_merged.pdf");
        File.WriteAllText(basePath, "dummy content 1");

        var uniquePath1 = _fileService.GetUniqueDestinationPath(_testDir, "sample", "pdf", "merged");
        Assert.Equal(Path.Combine(_testDir, "sample_merged (1).pdf"), uniquePath1);

        File.WriteAllText(uniquePath1, "dummy content 2");

        var uniquePath2 = _fileService.GetUniqueDestinationPath(_testDir, "sample", "pdf", "merged");
        Assert.Equal(Path.Combine(_testDir, "sample_merged (2).pdf"), uniquePath2);
    }

    [Fact]
    public void CreateTempFilePath_ReturnsValidTempPathInTempDirectory()
    {
        var tempPath = _fileService.CreateTempFilePath(".tmp");
        Assert.EndsWith(".tmp", tempPath);
        Assert.Contains("PdfImageToolkit", tempPath);
    }
}
