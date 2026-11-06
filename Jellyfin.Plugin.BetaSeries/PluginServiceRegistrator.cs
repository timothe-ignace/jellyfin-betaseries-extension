using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.BetaSeries;

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<TokenStore>();
        serviceCollection.AddSingleton<BetaSeriesClient>();
        serviceCollection.AddHostedService<WatchedSyncService>();
        serviceCollection.AddHostedService<ProfileMenuInjector>();
    }
}
