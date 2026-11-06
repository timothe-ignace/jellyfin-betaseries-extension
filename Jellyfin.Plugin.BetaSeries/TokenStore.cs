using System.Collections.Concurrent;
using System.Text.Json;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.BetaSeries;

/// <summary>Stores each Jellyfin user's BetaSeries token, separately from the admin configuration.</summary>
public class TokenStore
{
    private readonly string _path;
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<Guid, string> _tokens = new();

    public TokenStore(IApplicationPaths paths)
    {
        _path = Path.Combine(paths.PluginConfigurationsPath, "Jellyfin.Plugin.BetaSeries.tokens.json");
        try
        {
            if (File.Exists(_path))
            {
                var d = JsonSerializer.Deserialize<Dictionary<Guid, string>>(File.ReadAllText(_path));
                if (d is not null)
                {
                    foreach (var kv in d) { _tokens[kv.Key] = kv.Value; }
                }
            }
        }
        catch (Exception) { /* corrupted file: start from scratch */ }
    }

    public string? Get(Guid userId) => _tokens.TryGetValue(userId, out var t) ? t : null;

    public void Set(Guid userId, string token) { _tokens[userId] = token; Save(); }

    public void Remove(Guid userId) { _tokens.TryRemove(userId, out _); Save(); }

    private void Save()
    {
        lock (_lock)
        {
            // Atomic write: temporary file created directly with mode 0600, then renamed.
            var tmp = _path + ".tmp";
            var options = new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.Write };
            if (!OperatingSystem.IsWindows()) { options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite; }
            using (var fs = new FileStream(tmp, options))
            {
                JsonSerializer.Serialize(fs, _tokens.ToDictionary(k => k.Key, v => v.Value));
            }

            File.Move(tmp, _path, overwrite: true);
        }
    }
}
