using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.BetaSeries;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>Clé API de l'application BetaSeries (X-BetaSeries-Key).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Marquer aussi les films comme vus.</summary>
    public bool SyncMovies { get; set; } = true;
}
