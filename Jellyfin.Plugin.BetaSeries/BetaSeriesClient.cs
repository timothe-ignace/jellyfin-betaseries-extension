using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BetaSeries;

public class BetaSeriesClient
{
    private const string BaseUrl = "https://api.betaseries.com";
    private readonly IHttpClientFactory _http;
    private readonly ILogger<BetaSeriesClient> _log;

    public BetaSeriesClient(IHttpClientFactory http, ILogger<BetaSeriesClient> log)
    {
        _http = http;
        _log = log;
    }

    private static string ApiKey => Plugin.Instance?.Configuration.ApiKey ?? string.Empty;

    private async Task<(bool Ok, JsonDocument? Json, string Raw)> SendAsync(
        HttpMethod method, string path, IDictionary<string, string> form, string? token, CancellationToken ct)
    {
        using var client = _http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);
        using var req = new HttpRequestMessage(method, BaseUrl + path);
        req.Headers.Add("X-BetaSeries-Key", ApiKey);
        req.Headers.Add("X-BetaSeries-Version", "3.0");
        if (!string.IsNullOrEmpty(token)) { req.Headers.Add("X-BetaSeries-Token", token); }
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (method == HttpMethod.Get)
        {
            var q = string.Join("&", form.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            req.RequestUri = new Uri(BaseUrl + path + (q.Length > 0 ? "?" + q : string.Empty));
        }
        else
        {
            req.Content = new FormUrlEncodedContent(form);
        }

        using var resp = await client.SendAsync(req, ct).ConfigureAwait(false);
        var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        JsonDocument? json = null;
        try { json = JsonDocument.Parse(raw); } catch (JsonException) { }
        return (resp.IsSuccessStatusCode, json, raw);
    }

    /// <summary>Échange login/mot de passe contre un token. Le mot de passe n'est jamais conservé.</summary>
    public async Task<(string? Token, string? Error)> LoginAsync(string login, string password, CancellationToken ct)
    {
        var md5 = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
        var (ok, json, raw) = await SendAsync(HttpMethod.Post, "/members/auth",
            new Dictionary<string, string> { ["login"] = login, ["password"] = md5 }, null, ct).ConfigureAwait(false);
        if (ok && json is not null && json.RootElement.TryGetProperty("token", out var t))
        {
            return (t.GetString(), null);
        }

        return (null, ExtractError(json) ?? "Échec de connexion à BetaSeries.");
    }

    /// <summary>Vérifie un token et renvoie le login associé.</summary>
    public async Task<string?> GetLoginAsync(string token, CancellationToken ct)
    {
        var (ok, json, _) = await SendAsync(HttpMethod.Get, "/members/infos", new Dictionary<string, string>(), token, ct).ConfigureAwait(false);
        if (ok && json is not null && json.RootElement.TryGetProperty("member", out var m) && m.TryGetProperty("login", out var l))
        {
            return l.GetString();
        }

        return null;
    }

    /// <summary>Marque un épisode (ID TVDB de l'épisode) comme vu, sans toucher aux épisodes précédents.</summary>
    public async Task<bool> MarkEpisodeWatchedAsync(string token, string tvdbEpisodeId, CancellationToken ct)
    {
        var (ok, json, raw) = await SendAsync(HttpMethod.Post, "/episodes/watched",
            new Dictionary<string, string> { ["thetvdb_id"] = tvdbEpisodeId, ["bulk"] = "false" }, token, ct).ConfigureAwait(false);
        if (!ok)
        {
            // 2002 = épisode déjà vu : pas une vraie erreur
            if (ExtractCode(json) == 2002) { return true; }
            _log.LogWarning("BetaSeries episodes/watched a échoué : {Raw}", raw);
        }

        return ok;
    }

    /// <summary>Marque un film comme vu (recherche par ID IMDb ou TMDb).</summary>
    public async Task<bool> MarkMovieWatchedAsync(string token, string? imdbId, string? tmdbId, CancellationToken ct)
    {
        var q = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(imdbId)) { q["imdb_id"] = imdbId; }
        else if (!string.IsNullOrEmpty(tmdbId)) { q["tmdb_id"] = tmdbId; }
        else { return false; }

        var (ok, json, raw) = await SendAsync(HttpMethod.Get, "/movies/movie", q, token, ct).ConfigureAwait(false);
        if (!ok || json is null || !json.RootElement.TryGetProperty("movie", out var movie) || !movie.TryGetProperty("id", out var idEl))
        {
            _log.LogWarning("BetaSeries : film introuvable ({Raw})", raw);
            return false;
        }

        var (ok2, json2, raw2) = await SendAsync(HttpMethod.Post, "/movies/movie",
            new Dictionary<string, string> { ["id"] = idEl.ToString(), ["state"] = "1" }, token, ct).ConfigureAwait(false);
        if (!ok2 && ExtractCode(json2) != 4001)
        {
            _log.LogWarning("BetaSeries movies/movie a échoué : {Raw}", raw2);
            return false;
        }

        return true;
    }

    private static int? ExtractCode(JsonDocument? json)
    {
        if (json is not null && json.RootElement.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array && errs.GetArrayLength() > 0
            && errs[0].TryGetProperty("code", out var c) && c.TryGetInt32(out var code))
        {
            return code;
        }

        return null;
    }

    private static string? ExtractError(JsonDocument? json)
    {
        if (json is not null && json.RootElement.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array && errs.GetArrayLength() > 0
            && errs[0].TryGetProperty("text", out var t))
        {
            return t.GetString();
        }

        return null;
    }
}
