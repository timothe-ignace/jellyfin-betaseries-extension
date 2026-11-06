using System.Net.Mime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.BetaSeries.Api;

public class LoginRequest { public string Login { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }

public class TokenRequest { public string Token { get; set; } = string.Empty; }

/// <summary>Endpoints utilisés par la page de profil : chaque utilisateur ne gère que son propre token.</summary>
[ApiController]
[Authorize]
[Route("BetaSeries")]
[Produces(MediaTypeNames.Application.Json)]
public partial class BetaSeriesController : ControllerBase
{
    [GeneratedRegex("^[A-Za-z0-9._-]{8,256}$")]
    private static partial Regex TokenPattern();

    private readonly TokenStore _tokens;
    private readonly BetaSeriesClient _client;

    public BetaSeriesController(TokenStore tokens, BetaSeriesClient client)
    {
        _tokens = tokens;
        _client = client;
    }

    private Guid? CurrentUserId()
    {
        // Jellyfin place l'ID utilisateur dans le claim "Jellyfin-UserId"
        var raw = User.FindFirstValue("Jellyfin-UserId");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    [HttpGet("Status")]
    public async Task<ActionResult> Status(CancellationToken ct)
    {
        var uid = CurrentUserId();
        if (uid is null) { return Unauthorized(); }
        var configured = !string.IsNullOrEmpty(Plugin.Instance?.Configuration.ApiKey);
        var token = _tokens.Get(uid.Value);
        string? login = null;
        if (token is not null && configured) { login = await _client.GetLoginAsync(token, ct).ConfigureAwait(false); }
        return Ok(new { ServerConfigured = configured, Linked = token is not null, Valid = login is not null, Login = login });
    }

    [HttpPost("Login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var uid = CurrentUserId();
        if (uid is null) { return Unauthorized(); }
        if (string.IsNullOrEmpty(Plugin.Instance?.Configuration.ApiKey)) { return BadRequest(new { Error = "Le plugin n'est pas configuré (clé API manquante)." }); }
        if (string.IsNullOrWhiteSpace(req?.Login) || string.IsNullOrEmpty(req.Password) || req.Login.Length > 128 || req.Password.Length > 256)
        {
            return BadRequest(new { Error = "Identifiant ou mot de passe invalide." });
        }

        var (token, error) = await _client.LoginAsync(req.Login, req.Password, ct).ConfigureAwait(false);
        if (token is null) { return BadRequest(new { Error = error }); }
        _tokens.Set(uid.Value, token);
        return Ok(new { Login = req.Login });
    }

    [HttpPost("Token")]
    public async Task<ActionResult> SetToken([FromBody] TokenRequest req, CancellationToken ct)
    {
        var uid = CurrentUserId();
        if (uid is null) { return Unauthorized(); }
        if (string.IsNullOrEmpty(Plugin.Instance?.Configuration.ApiKey)) { return BadRequest(new { Error = "Le plugin n'est pas configuré (clé API manquante)." }); }
        var token = req?.Token?.Trim() ?? string.Empty;
        if (!TokenPattern().IsMatch(token)) { return BadRequest(new { Error = "Format de token invalide." }); }
        var login = await _client.GetLoginAsync(token, ct).ConfigureAwait(false);
        if (login is null) { return BadRequest(new { Error = "Token invalide." }); }
        _tokens.Set(uid.Value, token);
        return Ok(new { Login = login });
    }

    [HttpDelete("Token")]
    public ActionResult Unlink()
    {
        var uid = CurrentUserId();
        if (uid is null) { return Unauthorized(); }
        _tokens.Remove(uid.Value);
        return NoContent();
    }
}
