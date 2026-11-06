using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BetaSeries;

/// <summary>Écoute les changements d'état "vu" et les pousse vers BetaSeries.</summary>
public class WatchedSyncService : IHostedService
{
    private readonly IUserDataManager _userData;
    private readonly TokenStore _tokens;
    private readonly BetaSeriesClient _client;
    private readonly ILogger<WatchedSyncService> _log;

    public WatchedSyncService(IUserDataManager userData, TokenStore tokens, BetaSeriesClient client, ILogger<WatchedSyncService> log)
    {
        _userData = userData;
        _tokens = tokens;
        _client = client;
        _log = log;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _userData.UserDataSaved += OnUserDataSaved;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _userData.UserDataSaved -= OnUserDataSaved;
        return Task.CompletedTask;
    }

    private void OnUserDataSaved(object? sender, UserDataSaveEventArgs e)
    {
        if (e.SaveReason != UserDataSaveReason.PlaybackFinished && e.SaveReason != UserDataSaveReason.TogglePlayed)
        {
            return;
        }

        if (e.UserData is null || !e.UserData.Played)
        {
            return; // on ne synchronise que "marqué comme vu"
        }

        var token = _tokens.Get(e.UserId);
        if (token is null) { return; }

        var item = e.Item;
        _ = Task.Run(async () =>
        {
            try { await SyncAsync(item, token).ConfigureAwait(false); }
            catch (Exception ex) { _log.LogError(ex, "Erreur de synchro BetaSeries pour {Item}", item.Name); }
        });
    }

    private async Task SyncAsync(BaseItem item, string token)
    {
        var cfg = Plugin.Instance?.Configuration;
        if (cfg is null || string.IsNullOrEmpty(cfg.ApiKey)) { return; }

        if (item is Episode ep)
        {
            if (ep.TryGetProviderId(MetadataProvider.Tvdb, out var tvdb) && !string.IsNullOrEmpty(tvdb))
            {
                var ok = await _client.MarkEpisodeWatchedAsync(token, tvdb, CancellationToken.None).ConfigureAwait(false);
                _log.LogInformation("BetaSeries : {Series} S{S}E{E} -> {Result}", ep.SeriesName, ep.ParentIndexNumber, ep.IndexNumber, ok ? "ok" : "échec");
            }
            else
            {
                _log.LogInformation("BetaSeries : pas d'ID TVDB pour {Series} S{S}E{E}, ignoré", ep.SeriesName, ep.ParentIndexNumber, ep.IndexNumber);
            }
        }
        else if (item is Movie movie && cfg.SyncMovies)
        {
            movie.TryGetProviderId(MetadataProvider.Imdb, out var imdb);
            movie.TryGetProviderId(MetadataProvider.Tmdb, out var tmdb);
            var ok = await _client.MarkMovieWatchedAsync(token, imdb, tmdb, CancellationToken.None).ConfigureAwait(false);
            _log.LogInformation("BetaSeries : film {Movie} -> {Result}", movie.Name, ok ? "ok" : "échec");
        }
    }
}
