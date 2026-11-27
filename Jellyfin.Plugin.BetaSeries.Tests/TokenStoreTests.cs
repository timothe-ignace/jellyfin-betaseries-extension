using MediaBrowser.Common.Configuration;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.BetaSeries.Tests;

public sealed class TokenStoreTests : IDisposable
{
    private readonly DirectoryInfo _dir = Directory.CreateTempSubdirectory("betaseries-tests-");
    private readonly IApplicationPaths _paths = Substitute.For<IApplicationPaths>();

    public TokenStoreTests() => _paths.PluginConfigurationsPath.Returns(_dir.FullName);

    private string TokenFile => Path.Combine(_dir.FullName, "Jellyfin.Plugin.BetaSeries.tokens.json");

    public void Dispose() => _dir.Delete(recursive: true);

    [Fact]
    public void Tokens_survive_a_restart_and_stay_per_user()
    {
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        var store = new TokenStore(_paths);
        store.Set(alice, "alice-token");
        store.Set(bob, "bob-token");

        var reloaded = new TokenStore(_paths);

        Assert.Equal("alice-token", reloaded.Get(alice));
        Assert.Equal("bob-token", reloaded.Get(bob));
        Assert.Null(reloaded.Get(Guid.NewGuid()));
    }

    [Fact]
    public void Removed_token_stays_removed_after_a_restart()
    {
        var user = Guid.NewGuid();
        var store = new TokenStore(_paths);
        store.Set(user, "token");

        store.Remove(user);

        Assert.Null(new TokenStore(_paths).Get(user));
    }

    [Fact]
    public void Corrupted_file_is_ignored_and_can_be_overwritten()
    {
        File.WriteAllText(TokenFile, "{ not json");
        var user = Guid.NewGuid();

        var store = new TokenStore(_paths);
        Assert.Null(store.Get(user));

        store.Set(user, "token");
        Assert.Equal("token", new TokenStore(_paths).Get(user));
    }

    [Fact]
    public void Token_file_is_only_accessible_to_its_owner()
    {
        if (OperatingSystem.IsWindows()) { return; }

        new TokenStore(_paths).Set(Guid.NewGuid(), "token");

        Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(TokenFile));
    }
}
