using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.BetaSeries;

/// <summary>
/// Registers the profile-menu script with the "JavaScript Injector" plugin (via reflection),
/// to add a BetaSeries entry to each user's preferences menu.
/// </summary>
public class ProfileMenuInjector : IHostedService
{
    private const string ScriptId = "b7d3a1f2-5c64-4e8a-9a1d-3f0b6c2e7a91-profile-menu";
    private readonly ILogger<ProfileMenuInjector> _log;

    public ProfileMenuInjector(ILogger<ProfileMenuInjector> log) { _log = log; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            // Give the other plugins time to finish loading.
            await Task.Delay(TimeSpan.FromSeconds(5), CancellationToken.None).ConfigureAwait(false);
            try { Register(); }
            catch (Exception ex) { _log.LogWarning(ex, "BetaSeries: could not register the profile menu script"); }
        }, CancellationToken.None);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void Register()
    {
        var assembly = AssemblyLoadContext.All
            .SelectMany(c => c.Assemblies)
            .FirstOrDefault(a => a.FullName?.Contains(".JavaScriptInjector", StringComparison.Ordinal) ?? false);
        if (assembly is null)
        {
            _log.LogWarning("BetaSeries: the JavaScript Injector plugin is missing, the profile menu entry was not added " +
                            "(the page is still reachable at #/configurationpage?name=BetaSeriesUser).");
            return;
        }

        var script = ReadScript();
        var plugin = Plugin.Instance!;
        var payload = new JObject
        {
            ["id"] = ScriptId,
            ["name"] = "BetaSeries - profile menu",
            ["script"] = script,
            ["enabled"] = true,
            ["requiresAuthentication"] = true,
            ["pluginId"] = plugin.Id.ToString(),
            ["pluginName"] = plugin.Name,
            ["pluginVersion"] = plugin.Version.ToString()
        };

        var type = assembly.GetType("Jellyfin.Plugin.JavaScriptInjector.PluginInterface");
        var method = type?.GetMethod("RegisterScript", BindingFlags.Public | BindingFlags.Static);
        if (method is null)
        {
            _log.LogWarning("BetaSeries: RegisterScript API not found in JavaScript Injector.");
            return;
        }

        var result = method.Invoke(null, new object[] { payload });
        _log.LogInformation("BetaSeries: profile menu script registered ({Result})", result);
    }

    private static string ReadScript()
    {
        var name = typeof(Plugin).Namespace + ".Configuration.profileMenu.js";
        using var s = typeof(Plugin).Assembly.GetManifestResourceStream(name)!;
        using var r = new StreamReader(s);
        return r.ReadToEnd();
    }
}
