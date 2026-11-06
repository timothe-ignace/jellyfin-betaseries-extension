using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.BetaSeries;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>BetaSeries application API key (X-BetaSeries-Key).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Also mark movies as watched.</summary>
    public bool SyncMovies { get; set; } = true;
}
