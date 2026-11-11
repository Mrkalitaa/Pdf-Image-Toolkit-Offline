using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace PdfImageToolkit.Core.Models;

public partial class FileItem : ObservableObject
{
    public FileItem(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        
        try
        {
            var fileInfo = new FileInfo(filePath);
            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0;
        }
        catch
        {
            FileSizeBytes = 0;
        }
    }

    public string FilePath { get; }
    public string FileName { get; }
    public long FileSizeBytes { get; }

    [ObservableProperty]
    private int _pageCount;

    [ObservableProperty]
    private string _detailText = string.Empty;

    [ObservableProperty]
    private bool _isEncrypted;

    [ObservableProperty]
    private string? _password;

    public string FormattedSize => FormatBytes(FileSizeBytes);

    public static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {suffixes[order]}";
    }
}
