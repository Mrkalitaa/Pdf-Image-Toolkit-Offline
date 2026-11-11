using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using Xunit;

namespace PdfImageToolkit.Tests;

public class EntitlementTests : IDisposable
{
    private readonly string _testTempDir;
    private readonly ISettingsService _settingsService;
    private readonly ILoggingService _loggingService;

    public EntitlementTests()
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), "PdfImageToolkit_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testTempDir);

        _loggingService = new LoggingService();
        _settingsService = new SettingsService();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testTempDir))
            {
                Directory.Delete(_testTempDir, true);
            }
        }
        catch
        {
            // Ignore test cleanup error
        }
    }

    [Fact]
    public void FreeTier_AllowsInitialDailyOperations()
    {
        var service = new EntitlementService(_settingsService, _loggingService);
        service.SetSimulatedPro(false);

        Assert.False(service.IsPro);
        Assert.True(service.RemainingToday >= 0);
        Assert.True(service.RemainingToday <= 5);
    }

    [Fact]
    public void SimulatedPro_GrantsUnlimitedOperations()
    {
        var service = new EntitlementService(_settingsService, _loggingService);
        service.SetSimulatedPro(true);

        Assert.True(service.IsPro);
        Assert.Equal(int.MaxValue, service.RemainingToday);
        Assert.Equal(int.MaxValue, service.MaxBatchSize);
        Assert.Equal(long.MaxValue, service.MaxFileSizeBytes);
        Assert.True(service.TryConsumeOperation());
        Assert.True(service.CanProcessFile(100 * 1024 * 1024, out _));
        Assert.True(service.CanProcessBatch(50, out _));

        // Reset
        service.SetSimulatedPro(false);
    }

    [Fact]
    public void FreeTier_EnforcesFileAndBatchLimits()
    {
        var service = new EntitlementService(_settingsService, _loggingService);
        service.SetSimulatedPro(false);

        // 60 MB file should be rejected in Free tier
        var oversizedFile = 60L * 1024 * 1024;
        Assert.False(service.CanProcessFile(oversizedFile, out var fileReason));
        Assert.NotNull(fileReason);

        // 40 MB file should be allowed in Free tier
        var validFile = 40L * 1024 * 1024;
        Assert.True(service.CanProcessFile(validFile, out _));

        // 15 files should be rejected in Free tier
        Assert.False(service.CanProcessBatch(15, out var batchReason));
        Assert.NotNull(batchReason);

        // 5 files should be allowed in Free tier
        Assert.True(service.CanProcessBatch(5, out _));
    }
}
