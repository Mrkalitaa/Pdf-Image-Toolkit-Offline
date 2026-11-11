using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfImageToolkit.Core.Services;
using System;

namespace PdfImageToolkit.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IEntitlementService _entitlementService;

    public ShellViewModel(IEntitlementService entitlementService)
    {
        _entitlementService = entitlementService;
        UpdateStatus();
    }

    [ObservableProperty]
    private string _title = "PDF & Image Toolkit";

    [ObservableProperty]
    private bool _isPro;

    [ObservableProperty]
    private string _statusBadgeText = string.Empty;

    public void UpdateStatus()
    {
        IsPro = _entitlementService.IsPro;
        StatusBadgeText = IsPro 
            ? "PRO" 
            : $"{_entitlementService.RemainingToday} free left today";
    }
}
