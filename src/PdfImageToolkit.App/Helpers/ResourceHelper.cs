using Microsoft.Windows.ApplicationModel.Resources;

namespace PdfImageToolkit.Helpers;

public static class ResourceHelper
{
    private static ResourceLoader? _resourceLoader;

    private static ResourceLoader Loader => _resourceLoader ??= new ResourceLoader();

    public static string GetString(string key, string fallback = "")
    {
        try
        {
            var value = Loader.GetString(key);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
        catch
        {
            return fallback;
        }
    }
}
