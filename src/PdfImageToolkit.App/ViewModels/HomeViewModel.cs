using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PdfImageToolkit.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    public event Action<string, string>? NavigationRequested;

    [RelayCommand]
    private void OpenTool(string toolKey)
    {
        // Category and tab tag
        var parts = toolKey.Split(':');
        if (parts.Length == 2)
        {
            NavigationRequested?.Invoke(parts[0], parts[1]);
        }
    }
}
