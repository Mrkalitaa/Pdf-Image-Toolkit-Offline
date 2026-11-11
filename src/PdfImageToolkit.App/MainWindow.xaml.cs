using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace PdfImageToolkit;

public sealed partial class MainWindow : Window
{
    private AppWindow? _appWindow;

    public MainWindow()
    {
        InitializeComponent();

        var hwnd = WindowNative.GetWindowHandle(this);
        App.SetMainWindow(this, hwnd);

        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        if (_appWindow != null)
        {
            _appWindow.Title = "Pdf Image Toolkit: offline";
            _appWindow.Resize(new Windows.Graphics.SizeInt32(1100, 750));
        }
    }
}
