using System.Text.Json;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ChefsTest6.Services;

internal static class FixtureLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static async Task<T?> LoadAsync<T>(string fileName)
    {
        var uri = new Uri($"ms-appx:///Assets/Fixtures/{fileName}");
        try
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using var stream = await file.OpenStreamForReadAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, Options);
        }
        catch (Exception)
        {
            // Fallback: read from package install location.
            try
            {
                var path = System.IO.Path.Combine(
                    Package.Current.InstalledLocation.Path,
                    "Assets", "Fixtures", fileName);
                if (System.IO.File.Exists(path))
                {
                    using var stream = System.IO.File.OpenRead(path);
                    return await JsonSerializer.DeserializeAsync<T>(stream, Options);
                }
            }
            catch
            {
                // Swallow — return default below.
            }
            return default;
        }
    }
}
