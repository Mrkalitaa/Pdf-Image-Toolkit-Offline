using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PdfImageToolkit.ViewModels;

namespace PdfImageToolkit.Views;

public sealed partial class PdfToolsView : Page
{
    public PdfToolsViewModel ViewModel { get; }

    public PdfToolsView()
    {
        ViewModel = App.Services.GetRequiredService<PdfToolsViewModel>();
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
            "Merge" => new Pdf.MergeView(),
            "Split" => new Pdf.SplitView(),
            "Compress" => new Pdf.CompressView(),
            "Rotate" => new Pdf.RotateView(),
            "ImagesToPdf" => new Pdf.ImagesToPdfView(),
            "PdfToImages" => new Pdf.PdfToImagesView(),
            _ => new TextBlock
            {
                Text = $"PDF Tool: {tag} (Ready for next milestone)",
                FontSize = 16,
                Margin = new Microsoft.UI.Xaml.Thickness(16)
            }
        };
    }
}
