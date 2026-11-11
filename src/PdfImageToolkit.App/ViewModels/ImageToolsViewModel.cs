using CommunityToolkit.Mvvm.ComponentModel;

namespace PdfImageToolkit.ViewModels;

public partial class ImageToolsViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    public void SelectTabByTag(string tag)
    {
        SelectedTabIndex = tag switch
        {
            "Compress" => 0,
            "Resize" => 1,
            "Convert" => 2,
            _ => 0
        };
    }
}
