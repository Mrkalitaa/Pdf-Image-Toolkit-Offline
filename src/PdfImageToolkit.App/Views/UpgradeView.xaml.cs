using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PdfImageToolkit.ViewModels;

namespace PdfImageToolkit.Views;

public sealed partial class UpgradeView : Page
{
    public UpgradeViewModel ViewModel { get; }

    public UpgradeView()
    {
        ViewModel = App.Services.GetRequiredService<UpgradeViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshStatus();
    }
}
