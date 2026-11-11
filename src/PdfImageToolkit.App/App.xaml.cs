using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfImageToolkit.Core.Services;
using PdfImageToolkit.ViewModels;
using System;
using System.Threading.Tasks;

namespace PdfImageToolkit;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;
    public static nint MainWindowHandle { get; private set; }

    public static void SetMainWindow(Window window, nint hwnd)
    {
        MainWindow = window;
        MainWindowHandle = hwnd;
    }

    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IEntitlementService, EntitlementService>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IImageService, ImageService>();

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PdfToolsViewModel>();
        services.AddTransient<ImageToolsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<UpgradeViewModel>();

        // PDF ViewModels
        services.AddTransient<ViewModels.Pdf.MergeViewModel>();
        services.AddTransient<ViewModels.Pdf.SplitViewModel>();
        services.AddTransient<ViewModels.Pdf.RotateViewModel>();
        services.AddTransient<ViewModels.Pdf.ImagesToPdfViewModel>();
        services.AddTransient<ViewModels.Pdf.PdfToImagesViewModel>();
        services.AddTransient<ViewModels.Pdf.CompressViewModel>();

        // Image ViewModels
        services.AddTransient<ViewModels.Image.ImageCompressViewModel>();
        services.AddTransient<ViewModels.Image.ImageResizeViewModel>();
        services.AddTransient<ViewModels.Image.ImageConvertViewModel>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var settings = Services.GetRequiredService<ISettingsService>();
        var logging = Services.GetRequiredService<ILoggingService>();
        logging.LogInfo("Pdf Image Toolkit: offline application launching.");

        // Clean any leftover temp files on launch
        var fileService = Services.GetRequiredService<IFileService>();
        fileService.CleanupTempFiles();

        var window = new MainWindow();

        // Apply saved theme
        if (window.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = settings.AppTheme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }

        window.Activate();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        var logging = Services.GetService<ILoggingService>();
        logging?.LogError("Unhandled UI Exception caught.", e.Exception);
        e.Handled = true;

        _ = ShowExceptionDialogAsync(e.Exception);
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        var logging = Services.GetService<ILoggingService>();
        if (e.ExceptionObject is Exception ex)
        {
            logging?.LogError("Unhandled AppDomain Exception caught.", ex);
        }
        else
        {
            logging?.LogError($"Unhandled non-exception object: {e.ExceptionObject}");
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var logging = Services.GetService<ILoggingService>();
        logging?.LogError("Unobserved Task Exception caught.", e.Exception);
        e.SetObserved();
    }

    private async Task ShowExceptionDialogAsync(Exception ex)
    {
        try
        {
            if (MainWindow?.Content?.XamlRoot != null)
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = MainWindow.Content.XamlRoot,
                    Title = "Unexpected Error",
                    Content = $"An error occurred: {ex.Message}\n\nA diagnostic log has been written to your local app data folder.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                await dialog.ShowAsync();
            }
        }
        catch
        {
            // Do not crash if dialog cannot be displayed
        }
    }
}
