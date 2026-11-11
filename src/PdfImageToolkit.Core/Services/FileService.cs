using System;
using System.IO;

namespace PdfImageToolkit.Core.Services;

public interface IFileService
{
    string GetUniqueDestinationPath(string destinationFolder, string baseFileNameWithoutExtension, string extension, string suffix);
    string CreateTempFilePath(string extension);
    void CleanupTempFiles();
    string GetDefaultOutputDirectory();
    bool FileExists(string path);
    long GetFileSize(string path);
}

public class FileService : IFileService
{
    private readonly string _tempDirectory;
    private readonly ISettingsService _settingsService;

    public FileService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _tempDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PdfImageToolkit",
            "Temp");

        try
        {
            Directory.CreateDirectory(_tempDirectory);
        }
        catch
        {
            // Ignore if creation fails temporarily
        }
    }

    public string GetDefaultOutputDirectory()
    {
        var configured = _settingsService.DefaultOutputFolder;
        if (!string.IsNullOrWhiteSpace(configured) && Directory.Exists(configured))
        {
            return configured;
        }

        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PDF & Image Toolkit");

        try
        {
            Directory.CreateDirectory(defaultPath);
        }
        catch
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        return defaultPath;
    }

    public string GetUniqueDestinationPath(string destinationFolder, string baseFileNameWithoutExtension, string extension, string suffix)
    {
        if (string.IsNullOrWhiteSpace(destinationFolder) || !Directory.Exists(destinationFolder))
        {
            destinationFolder = GetDefaultOutputDirectory();
        }

        var normalizedExt = extension.StartsWith('.') ? extension : "." + extension;
        var baseNameWithSuffix = string.IsNullOrEmpty(suffix) 
            ? baseFileNameWithoutExtension 
            : $"{baseFileNameWithoutExtension}_{suffix}";

        var candidatePath = Path.Combine(destinationFolder, $"{baseNameWithSuffix}{normalizedExt}");
        if (!File.Exists(candidatePath))
        {
            return candidatePath;
        }

        int counter = 1;
        while (true)
        {
            candidatePath = Path.Combine(destinationFolder, $"{baseNameWithSuffix} ({counter}){normalizedExt}");
            if (!File.Exists(candidatePath))
            {
                return candidatePath;
            }
            counter++;
        }
    }

    public string CreateTempFilePath(string extension)
    {
        var ext = extension.StartsWith('.') ? extension : "." + extension;
        var fileName = $"{Guid.NewGuid():N}{ext}";
        return Path.Combine(_tempDirectory, fileName);
    }

    public void CleanupTempFiles()
    {
        try
        {
            if (!Directory.Exists(_tempDirectory)) return;

            var files = Directory.GetFiles(_tempDirectory);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Locked temp files will be cleaned on next launch
                }
            }
        }
        catch
        {
            // Suppress cleanup exceptions
        }
    }

    public bool FileExists(string path) => File.Exists(path);

    public long GetFileSize(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0;
        }
    }
}
