using CommunityToolkit.Mvvm.ComponentModel;

namespace PdfImageToolkit.ViewModels;

public partial class PdfToolsViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    public void SelectTabByTag(string tag)
    {
        SelectedTabIndex = tag switch
        {
            "Merge" => 0,
            "Split" => 1,
            "Compress" => 2,
            "Rotate" => 3,
            "ImagesToPdf" => 4,
            "PdfToImages" => 5,
            _ => 0
        };
    }
}
