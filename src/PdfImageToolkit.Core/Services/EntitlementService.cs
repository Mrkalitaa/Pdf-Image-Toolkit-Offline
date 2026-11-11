using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PdfImageToolkit.Core.Services;

public interface IEntitlementService
{
    bool IsPro { get; }
    int RemainingToday { get; }
    int MaxBatchSize { get; }
    long MaxFileSizeBytes { get; }
    bool TryConsumeOperation();
    bool CanProcessFile(long fileSizeBytes, out string? reason);
    bool CanProcessBatch(int fileCount, out string? reason);
    Task<bool> RefreshEntitlementsAsync();
    Task<bool> PurchaseProAsync(nint windowHandle);
    Task<string?> GetProFormattedPriceAsync();
    void SetSimulatedPro(bool enabled);
}

public class EntitlementService : IEntitlementService
{
    public const int FreeDailyOperations = 5;
    public const int FreeMaxBatchSize = 10;
    public const long FreeMaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    public const string ProAddOnStoreId = "pro_unlock";

    private readonly ISettingsService _settingsService;
    private readonly ILoggingService _loggingService;
    private readonly string _usageFilePath;
    private readonly object _lock = new();

    private bool _storeProPurchased;
    private int _usedToday;
    private string _currentDateString;

    public EntitlementService(ISettingsService settingsService, ILoggingService loggingService)
    {
        _settingsService = settingsService;
        _loggingService = loggingService;

        var localDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PdfImageToolkit");
        Directory.CreateDirectory(localDir);
        _usageFilePath = Path.Combine(localDir, "usage.json");

        _currentDateString = DateTime.Today.ToString("yyyy-MM-dd");
        LoadDailyUsage();
    }

    public bool IsPro
    {
        get
        {
#if DEBUG
            if (_settingsService.SimulatedPro) return true;
#endif
            return _storeProPurchased;
        }
    }

    public int RemainingToday
    {
        get
        {
            if (IsPro) return int.MaxValue;
            EnsureDateCurrent();
            return Math.Max(0, FreeDailyOperations - _usedToday);
        }
    }

    public int MaxBatchSize => IsPro ? int.MaxValue : FreeMaxBatchSize;

    public long MaxFileSizeBytes => IsPro ? long.MaxValue : FreeMaxFileSizeBytes;

    public bool TryConsumeOperation()
    {
        if (IsPro) return true;

        lock (_lock)
        {
            EnsureDateCurrent();
            if (_usedToday >= FreeDailyOperations)
            {
                return false;
            }

            _usedToday++;
            SaveDailyUsage();
            return true;
        }
    }

    public bool CanProcessFile(long fileSizeBytes, out string? reason)
    {
        if (IsPro || fileSizeBytes <= MaxFileSizeBytes)
        {
            reason = null;
            return true;
        }

        reason = $"File size ({Models.FileItem.FormatBytes(fileSizeBytes)}) exceeds the Free tier limit of 50 MB. Upgrade to Pro for unlimited file size.";
        return false;
    }

    public bool CanProcessBatch(int fileCount, out string? reason)
    {
        if (IsPro || fileCount <= MaxBatchSize)
        {
            reason = null;
            return true;
        }

        reason = $"Batch count ({fileCount}) exceeds the Free tier limit of 10 files. Upgrade to Pro for unlimited batch processing.";
        return false;
    }

    public void SetSimulatedPro(bool enabled)
    {
        _settingsService.SimulatedPro = enabled;
        _settingsService.Save();
    }

    public async Task<bool> RefreshEntitlementsAsync()
    {
        try
        {
            // Windows.Services.Store.StoreContext check
            var context = Windows.Services.Store.StoreContext.GetDefault();
            var appLicense = await context.GetAppLicenseAsync();

            if (appLicense.AddOnLicenses.TryGetValue(ProAddOnStoreId, out var addOnLicense) && addOnLicense.IsActive)
            {
                _storeProPurchased = true;
                _loggingService.LogInfo("Pro add-on active license confirmed from Microsoft Store.");
                return true;
            }

            _storeProPurchased = false;
            return false;
        }
        catch (Exception ex)
        {
            // StoreContext throws when unpackaged or offline
            _loggingService.LogWarning($"StoreContext check encountered offline/unpackaged state: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PurchaseProAsync(nint windowHandle)
    {
        try
        {
            var context = Windows.Services.Store.StoreContext.GetDefault();

            // Initialize with Window handle for MSIX WinUI 3 dialog
            if (windowHandle != 0)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(context, windowHandle);
            }

            var result = await context.RequestPurchaseAsync(ProAddOnStoreId);

            if (result.Status == Windows.Services.Store.StorePurchaseStatus.Succeeded ||
                result.Status == Windows.Services.Store.StorePurchaseStatus.AlreadyPurchased)
            {
                _storeProPurchased = true;
                _loggingService.LogInfo("Pro add-on purchase succeeded.");
                return true;
            }

            _loggingService.LogWarning($"Pro purchase status: {result.Status}, ExtendedError: {result.ExtendedError?.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Pro purchase request failed.", ex);
            return false;
        }
    }

    public async Task<string?> GetProFormattedPriceAsync()
    {
        try
        {
            var context = Windows.Services.Store.StoreContext.GetDefault();
            var productsResult = await context.GetAssociatedStoreProductsAsync(new[] { "Durable" });
            if (productsResult.ExtendedError == null && 
                productsResult.Products.TryGetValue(ProAddOnStoreId, out var product))
            {
                return product.Price?.FormattedPrice;
            }
        }
        catch
        {
            // Graceful fallback when running offline or unpackaged
        }

        return null;
    }

    private void EnsureDateCurrent()
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        if (_currentDateString != today)
        {
            _currentDateString = today;
            _usedToday = 0;
            SaveDailyUsage();
        }
    }

    private void LoadDailyUsage()
    {
        try
        {
            if (File.Exists(_usageFilePath))
            {
                var json = File.ReadAllText(_usageFilePath);
                var data = JsonSerializer.Deserialize<UsageData>(json);
                if (data != null && data.Date == _currentDateString)
                {
                    _usedToday = data.Count;
                    return;
                }
            }
        }
        catch
        {
            // Reset on error
        }

        _usedToday = 0;
        SaveDailyUsage();
    }

    private void SaveDailyUsage()
    {
        try
        {
            var data = new UsageData
            {
                Date = _currentDateString,
                Count = _usedToday
            };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_usageFilePath, json);
        }
        catch
        {
            // Ignore
        }
    }

    private class UsageData
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
