using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfImageToolkit.ViewModels.Image;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace PdfImageToolkit.Views.Image;

public sealed partial class ImageConvertView : UserControl
{
    public ImageConvertViewModel ViewModel { get; }

    public ImageConvertView()
    {
        ViewModel = App.Services.GetRequiredService<ImageConvertViewModel>();
        InitializeComponent();
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "Add image files to convert";
        e.DragUIOverride.IsCaptionVisible = true;
        e.DragUIOverride.IsGlyphVisible = true;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var files = items.OfType<StorageFile>().Select(f => f.Path).ToList();
            if (files.Count > 0)
            {
                ViewModel.AddImages(files);
            }
        }
    }
}
