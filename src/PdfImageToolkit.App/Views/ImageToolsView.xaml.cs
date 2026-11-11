using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PdfImageToolkit.ViewModels;

namespace PdfImageToolkit.Views;

public sealed partial class ImageToolsView : Page
{
    public ImageToolsViewModel ViewModel { get; }

    public ImageToolsView()
    {
        ViewModel = App.Services.GetRequiredService<ImageToolsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string tabTag)
        {
            SelectTab(tabTag);
        }
        else if (ToolNav.SelectedItem == null && ToolNav.MenuItems.Count > 0)
        {
            ToolNav.SelectedItem = ToolNav.MenuItems[0];
        }
    }

    public void SelectTab(string tabTag)
    {
        foreach (var item in ToolNav.MenuItems)
        {
            if (item is NavigationViewItem navItem && (string)navItem.Tag == tabTag)
            {
                ToolNav.SelectedItem = navItem;
                break;
            }
        }
    }

    private void ToolNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            LoadToolView(tag);
        }
    }

    public void LoadToolView(string tag)
    {
        ActiveToolContent.Content = tag switch
        {
            "Compress" => new Image.ImageCompressView(),
            "Resize" => new Image.ImageResizeView(),
            "Convert" => new Image.ImageConvertView(),
            _ => new TextBlock
            {
                Text = $"Image Tool: {tag}",
                FontSize = 16,
                Margin = new Microsoft.UI.Xaml.Thickness(16)
            }
        };
    }
}
