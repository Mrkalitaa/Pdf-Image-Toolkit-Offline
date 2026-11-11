using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfImageToolkit.Core.Services;
using System;
using System.Threading.Tasks;

namespace PdfImageToolkit.ViewModels;

public partial class UpgradeViewModel : ObservableObject
{
    private readonly IEntitlementService _entitlementService;
    private readonly ILoggingService _loggingService;

    public UpgradeViewModel(IEntitlementService entitlementService, ILoggingService loggingService)
    {
        _entitlementService = entitlementService;
        _loggingService = loggingService;
        RefreshStatus();
        _ = LoadPriceAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPurchase))]
    [NotifyPropertyChangedFor(nameof(FreeQuotaVisibility))]
    [NotifyPropertyChangedFor(nameof(LicenseStatusText))]
    private bool _isPro;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PurchaseButtonText))]
    private string? _storeFormattedPrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RemainingTodayFormatted))]
    private int _remainingToday;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPurchase))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    private string? _statusMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MessageSeverity))]
    private bool _isError;

    public bool CanPurchase => !IsPro && !IsBusy;
    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);
    public Visibility FreeQuotaVisibility => IsPro ? Visibility.Collapsed : Visibility.Visible;
    public InfoBarSeverity MessageSeverity => IsError ? InfoBarSeverity.Error : InfoBarSeverity.Success;
    public string RemainingTodayFormatted => $"Operations remaining today: {RemainingToday} (Resets at midnight)";
    public string LicenseStatusText => IsPro ? "You have the Pro Edition. All limits are unlocked!" : "You are currently using the Free Tier (5 operations per day).";
    public string PurchaseButtonText => string.IsNullOrWhiteSpace(StoreFormattedPrice) 
        ? "Unlock Pro Now" 
        : $"Unlock Pro Now — {StoreFormattedPrice}";

    public void RefreshStatus()
    {
        IsPro = _entitlementService.IsPro;
        RemainingToday = _entitlementService.RemainingToday;
    }

    private async Task LoadPriceAsync()
    {
        try
        {
            var price = await _entitlementService.GetProFormattedPriceAsync();
            if (!string.IsNullOrEmpty(price))
            {
                StoreFormattedPrice = price;
            }
        }
        catch
        {
            // Ignore offline
        }
    }

    [RelayCommand]
    private async Task PurchaseProAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = null;
        IsError = false;

        try
        {
            var success = await _entitlementService.PurchaseProAsync(App.MainWindowHandle);
            if (success)
            {
                RefreshStatus();
                StatusMessage = "Congratulations! Pro Edition has been unlocked successfully.";
                IsError = false;
            }
            else
            {
                StatusMessage = "Purchase was not completed. If you are offline, please connect to the internet and try again.";
                IsError = true;
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during Pro purchase flow", ex);
            StatusMessage = "An error occurred while connecting to Microsoft Store. Please try again later.";
            IsError = true;
        }
        finally
        {
            IsBusy = false;
            RefreshStatus();
        }
    }
}
