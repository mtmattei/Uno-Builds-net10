using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ChefsTest6.Services;

internal static class DataLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static T Load<T>(string fileName)
    {
        var asm = typeof(DataLoader).Assembly;
        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".Data.{fileName}", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Embedded resource not found for {fileName}");

        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not open resource {resourceName}");

        using var reader = new StreamReader(stream);
        var raw = reader.ReadToEnd();

        // Strip BOM if present
        if (raw.Length > 0 && raw[0] == '﻿') raw = raw[1..];

        return JsonSerializer.Deserialize<T>(raw, Options)!;
    }
}
