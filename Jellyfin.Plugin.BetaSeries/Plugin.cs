using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.BetaSeries;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "BetaSeries";

    public override Guid Id => Guid.Parse("b7d3a1f2-5c64-4e8a-9a1d-3f0b6c2e7a91");

    public override string Description => "Syncs watched episodes and movies to BetaSeries (multi-user).";

    public IEnumerable<PluginPageInfo> GetPages()
    {
        var ns = GetType().Namespace;
        yield return new PluginPageInfo
        {
            Name = "BetaSeries",
            EmbeddedResourcePath = $"{ns}.Configuration.configPage.html"
        };
        yield return new PluginPageInfo
        {
            Name = "BetaSeriesUser",
            EmbeddedResourcePath = $"{ns}.Configuration.userPage.html"
        };
    }
}
