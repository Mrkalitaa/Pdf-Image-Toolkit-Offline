using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfImageToolkit.ViewModels;

namespace PdfImageToolkit.Views;

public sealed partial class HomeView : Page
{
    public HomeViewModel ViewModel { get; }

    public HomeView()
    {
        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
        InitializeComponent();
    }

    private void ToolCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            var parts = tag.Split(':');
            if (parts.Length == 2)
            {
                var category = parts[0];
                var subTab = parts[1];

                if (App.MainWindow.Content is ShellView shell)
                {
                    shell.NavigateTo(category, subTab);
                }
            }
        }
    }
}
