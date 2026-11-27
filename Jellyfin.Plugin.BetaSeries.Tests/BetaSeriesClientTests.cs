using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.BetaSeries.Tests;

public class BetaSeriesClientTests
{
    private readonly FakeHandler _api = new();
    private readonly BetaSeriesClient _client;

    public BetaSeriesClientTests()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(_api, disposeHandler: false));
        _client = new BetaSeriesClient(factory, NullLogger<BetaSeriesClient>.Instance);
    }

    [Fact]
    public async Task Login_sends_the_password_hash_and_returns_the_token()
    {
        _api.Reply(HttpStatusCode.OK, """{"token":"abc"}""");

        var (token, error) = await _client.LoginAsync("me", "password", CancellationToken.None);

        Assert.Equal("abc", token);
        Assert.Null(error);
        // MD5 of "password", as the API expects.
        Assert.Equal("login=me&password=5f4dcc3b5aa765d61d8327deb882cf99", Assert.Single(_api.Requests).Body);
    }

    [Fact]
    public async Task Login_reports_the_api_error()
    {
        _api.Reply(HttpStatusCode.BadRequest, """{"errors":[{"code":4003,"text":"Wrong password"}]}""");

        var (token, error) = await _client.LoginAsync("me", "nope", CancellationToken.None);

        Assert.Null(token);
        Assert.Equal("Wrong password", error);
    }

    [Fact]
    public async Task Login_survives_a_response_that_is_not_json()
    {
        _api.Reply(HttpStatusCode.BadGateway, "<html>Bad gateway</html>");

        var (token, error) = await _client.LoginAsync("me", "password", CancellationToken.None);

        Assert.Null(token);
        Assert.Equal("Could not sign in to BetaSeries.", error);
    }

    [Fact]
    public async Task Episode_is_marked_without_checking_previous_ones()
    {
        _api.Reply(HttpStatusCode.OK, "{}");

        Assert.True(await _client.MarkEpisodeWatchedAsync("token", "42", CancellationToken.None));

        var request = Assert.Single(_api.Requests);
        Assert.Equal("https://api.betaseries.com/episodes/watched", request.Url);
        Assert.Equal("thetvdb_id=42&bulk=false", request.Body);
        Assert.Equal("token", request.Token);
    }

    [Theory]
    [InlineData(2002, true)] // already watched
    [InlineData(2001, false)]
    public async Task Episode_error_codes_are_interpreted(int code, bool expected)
    {
        _api.Reply(HttpStatusCode.BadRequest, $$"""{"errors":[{"code":{{code}}}]}""");

        Assert.Equal(expected, await _client.MarkEpisodeWatchedAsync("token", "42", CancellationToken.None));
    }

    [Theory]
    [InlineData("tt1", "2", "imdb_id=tt1")] // IMDb wins
    [InlineData(null, "2", "tmdb_id=2")]
    [InlineData("", "2", "tmdb_id=2")]
    public async Task Movie_is_looked_up_by_imdb_then_tmdb(string? imdb, string? tmdb, string query)
    {
        _api.Reply(HttpStatusCode.OK, """{"movie":{"id":7}}""");
        _api.Reply(HttpStatusCode.OK, "{}");

        Assert.True(await _client.MarkMovieWatchedAsync("token", imdb, tmdb, CancellationToken.None));

        Assert.Equal("https://api.betaseries.com/movies/movie?" + query, _api.Requests[0].Url);
        Assert.Equal("id=7&state=1", _api.Requests[1].Body);
    }

    [Fact]
    public async Task Movie_without_identifier_makes_no_request()
    {
        Assert.False(await _client.MarkMovieWatchedAsync("token", null, null, CancellationToken.None));
        Assert.Empty(_api.Requests);
    }

    [Fact]
    public async Task Unknown_movie_is_not_marked()
    {
        _api.Reply(HttpStatusCode.NotFound, """{"errors":[{"code":4001}]}""");

        Assert.False(await _client.MarkMovieWatchedAsync("token", "tt1", null, CancellationToken.None));
        Assert.Single(_api.Requests);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Queue<(HttpStatusCode Status, string Body)> _replies = new();

        public List<(string Url, string? Body, string? Token)> Requests { get; } = new();

        public void Reply(HttpStatusCode status, string body) => _replies.Enqueue((status, body));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            var token = request.Headers.TryGetValues("X-BetaSeries-Token", out var values) ? values.Single() : null;
            Requests.Add((request.RequestUri!.AbsoluteUri, body, token));
            var reply = _replies.Dequeue();
            return new HttpResponseMessage(reply.Status) { Content = new StringContent(reply.Body) };
        }
    }
}
