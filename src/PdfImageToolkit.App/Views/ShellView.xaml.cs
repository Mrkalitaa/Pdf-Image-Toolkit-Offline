using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfImageToolkit.ViewModels;
using System;

namespace PdfImageToolkit.Views;

public sealed partial class ShellView : UserControl
{
    public ShellViewModel ViewModel { get; }

    public ShellView()
    {
        ViewModel = App.Services.GetRequiredService<ShellViewModel>();
        InitializeComponent();
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = HomeNavItem;
        NavigateTo("Home");
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    public void NavigateTo(string tag, object? parameter = null)
    {
        Type pageType = tag switch
        {
            "Home" => typeof(HomeView),
            "PdfTools" => typeof(PdfToolsView),
            "ImageTools" => typeof(ImageToolsView),
            "Settings" => typeof(SettingsView),
            "Upgrade" => typeof(UpgradeView),
            _ => typeof(HomeView)
        };

        if (ContentFrame.CurrentSourcePageType != pageType || parameter != null)
        {
            ContentFrame.Navigate(pageType, parameter);
        }

        // Keep selection synced
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem && (string)navItem.Tag == tag)
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
        foreach (var item in NavView.FooterMenuItems)
        {
            if (item is NavigationViewItem navItem && (string)navItem.Tag == tag)
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }

        ViewModel.UpdateStatus();
    }
}
