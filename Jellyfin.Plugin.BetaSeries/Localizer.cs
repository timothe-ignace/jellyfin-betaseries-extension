using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.BetaSeries;

/// <summary>
/// Serves the web UI translations embedded in the plugin (Configuration/i18n/&lt;lang&gt;.json).
/// English is the base: any missing key in another language falls back to it.
/// </summary>
public static partial class Localizer
{
    private const string Prefix = "Jellyfin.Plugin.BetaSeries.Configuration.i18n.";
    private const string Fallback = "en";

    [GeneratedRegex("^[A-Za-z]{2,3}([-_][A-Za-z0-9]{2,8})*$")]
    private static partial Regex LangPattern();

    /// <summary>Returns the translations for a culture such as "fr", "fr-FR" or "pt_BR" as a JSON object.</summary>
    public static string Load(string? culture)
    {
        var result = ReadResource(Fallback) ?? new JsonObject();
        var code = Resolve(culture);
        if (code != Fallback && ReadResource(code) is { } specific)
        {
            foreach (var kv in specific)
            {
                result[kv.Key] = kv.Value?.DeepClone();
            }
        }

        return result.ToJsonString();
    }

    private static string Resolve(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture) || !LangPattern().IsMatch(culture))
        {
            return Fallback;
        }

        var full = culture.Replace('_', '-').ToLowerInvariant();
        var primary = full.Split('-')[0];
        return Available().Contains(full) ? full : Available().Contains(primary) ? primary : Fallback;
    }

    private static HashSet<string> Available() => typeof(Localizer).Assembly.GetManifestResourceNames()
        .Where(n => n.StartsWith(Prefix, StringComparison.Ordinal) && n.EndsWith(".json", StringComparison.Ordinal))
        .Select(n => n[Prefix.Length..^".json".Length].ToLowerInvariant())
        .ToHashSet();

    private static JsonObject? ReadResource(string code)
    {
        var assembly = typeof(Localizer).Assembly;
        var name = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => string.Equals(n, Prefix + code + ".json", StringComparison.OrdinalIgnoreCase));
        if (name is null) { return null; }
        using var stream = assembly.GetManifestResourceStream(name)!;
        return JsonNode.Parse(stream) as JsonObject;
    }
}
