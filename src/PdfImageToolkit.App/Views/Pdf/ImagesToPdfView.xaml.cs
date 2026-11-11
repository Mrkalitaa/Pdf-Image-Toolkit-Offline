using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfImageToolkit.ViewModels.Pdf;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace PdfImageToolkit.Views.Pdf;

public sealed partial class ImagesToPdfView : UserControl
{
    public ImagesToPdfViewModel ViewModel { get; }

    public ImagesToPdfView()
    {
        ViewModel = App.Services.GetRequiredService<ImagesToPdfViewModel>();
        InitializeComponent();
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "Add images to convert to PDF";
        e.DragUIOverride.IsCaptionVisible = true;
        e.DragUIOverride.IsGlyphVisible = true;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var paths = items.OfType<StorageFile>().Select(f => f.Path).ToList();
            ViewModel.AddImages(paths);
        }
    }
}
