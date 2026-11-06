# Jellyfin BetaSeries

[![CI](https://github.com/timothe-ignace/jellyfin-betaseries-extension/actions/workflows/ci.yml/badge.svg)](https://github.com/timothe-ignace/jellyfin-betaseries-extension/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/timothe-ignace/jellyfin-betaseries-extension)](https://github.com/timothe-ignace/jellyfin-betaseries-extension/releases)
[![License](https://img.shields.io/github/license/timothe-ignace/jellyfin-betaseries-extension)](LICENSE)

A [Jellyfin](https://jellyfin.org) plugin that automatically marks the **episodes and movies you have watched** as seen on [BetaSeries](https://www.betaseries.com).

- **Multi-user**: every Jellyfin user links their own BetaSeries account from their own settings.
- **Automatic**: as soon as an episode or movie is finished (or manually marked as watched), it is checked off on BetaSeries.
- **Your password is never stored**: only the BetaSeries token is kept.

> Compatible with **Jellyfin 12.x**. See [Compatibility](#compatibility).

---

## Installation

### 1. Add the plugin repository

In Jellyfin, as an administrator:

1. **Dashboard → Plugins → Repositories → Add**
2. Name: `BetaSeries`
3. Repository URL:
   ```
   https://raw.githubusercontent.com/timothe-ignace/jellyfin-betaseries-extension/main/manifest.json
   ```

### 2. Install the plugin

1. **Dashboard → Plugins → Catalog**, find **BetaSeries** and install the latest version.
2. **Restart Jellyfin.**

### 3. Configure the API key (administrator, once)

BetaSeries identifies applications by an API key.

1. Create an application at <https://www.betaseries.com/api/> and copy its **Client ID**.
2. In Jellyfin: **Dashboard → Plugins → BetaSeries → Settings**, paste the key and save.

This key only identifies the application: it gives no access to any account and is shared by all users.

### 4. Link an account (each user)

Open your Jellyfin **Settings** (user menu): a **BetaSeries** entry appears there. Then either:

- sign in with your BetaSeries login and password (used once to obtain a token, never stored), or
- paste a BetaSeries token directly.

That's it: your next views are synced. To stop, click **Unlink my account** on the same page.

> **No BetaSeries entry in the menu?** Adding it to the settings menu relies on the [JavaScript Injector](https://github.com/n00bcodr/Jellyfin-JavaScript-Injector) plugin (install and enable it). Without it, the page is still reachable at
> `https://<your-jellyfin>/web/#/configurationpage?name=BetaSeriesUser`.

---

## What gets synced

| Item | Condition | Identifier used |
|---|---|---|
| Watched episode | finished, or marked as watched | the episode's **TVDB** ID |
| Watched movie (can be disabled by the admin) | finished, or marked as watched | **IMDb** ID, falling back to **TMDb** |

- Only **new** views are sent: your existing history is not imported.
- Marking an item as unwatched in Jellyfin does **not** uncheck it on BetaSeries.
- Marking an episode does **not** check the previous episodes on BetaSeries.
- Items without a TVDB/IMDb/TMDb identifier (missing metadata) are skipped; a log line tells you so.

## Troubleshooting

Search for `BetaSeries` in the Jellyfin logs (*Dashboard → Logs*, or `docker logs jellyfin | grep BetaSeries`).

| Symptom | What to check |
|---|---|
| "The plugin has not been configured by the administrator yet" | The admin has not entered the API key (step 3). |
| "Saved token is invalid or expired" | Sign in again from your BetaSeries page. |
| Nothing happens after watching something | Account not linked, or the episode has no TVDB ID: check the logs. |
| No BetaSeries entry in the settings menu | JavaScript Injector missing/disabled, or hard-reload the page (Ctrl+F5). |

## Security and privacy

- BetaSeries passwords are never stored nor logged.
- Each user's token is stored **in clear text** in `config/plugins/configurations/Jellyfin.Plugin.BetaSeries.tokens.json` (mode `0600`). Protect your Jellyfin configuration folder like the rest of your installation.
- Each user can only read and change their own token. The API key is never returned to non-administrators.
- Tokens of deleted Jellyfin users are not purged automatically.
- The only external service contacted is `api.betaseries.com`.

Found a vulnerability? Please open a [private report](https://github.com/timothe-ignace/jellyfin-betaseries-extension/security/advisories/new).

## Compatibility

| Plugin | Jellyfin |
|---|---|
| 1.x | 12.1+ (tested on 12.4) |

The plugin is built for a specific Jellyfin version (`targetAbi`). After a major Jellyfin upgrade, wait for a new plugin release.

---

## Contributing / development

Requirements: Docker (the .NET 10 SDK comes from the official image) or a local .NET 10 SDK.

```bash
./build.sh                                   # build → dist/BetaSeries_<version>/ and .zip
./build.sh ~/docker/jellyfin/config/plugins  # + copy into Jellyfin's plugins folder
docker restart jellyfin
```

Layout:

```
Jellyfin.Plugin.BetaSeries/   plugin code (C#) and web pages (Configuration/)
meta.json                     plugin metadata (version, targetAbi…)
manifest.json                 plugin repository manifest (generated by CI)
scripts/                      packaging and manifest update
.github/workflows/            CI and release
```

### Git workflow

- Work on a branch (`feat/…`, `fix/…`) and open a pull request against `main`; CI builds (warnings as errors) and packages.
- Short, imperative commit messages.

### Releasing

Versions have **4 parts**, like every Jellyfin plugin: `MAJOR.MINOR.PATCH.BUILD` (e.g. `1.2.0.0`). The **git tag** is the same, prefixed with `v` (`v1.2.0.0`).

1. In a PR: set `version` to `X.Y.Z.W` in `meta.json` **and** `AssemblyVersion`/`FileVersion` in the `.csproj` (CI checks they match); adjust `targetAbi` if needed; update `CHANGELOG.md`.
2. Merge the PR, then tag `main`:
   ```bash
   git switch main && git pull
   git tag vX.Y.Z.W && git push origin vX.Y.Z.W
   ```
3. CI (`release.yml`) builds, creates the GitHub release with the zip, then commits `manifest.json` (URL + MD5 checksum) to `main`. The plugin is then offered in the Jellyfin catalog.

`./release.sh <user/repo>` reproduces the zip and manifest locally (useful for testing).

### Targeting a new Jellyfin version

Adjust `Jellyfin.Controller` in the `.csproj` (deliberately pinned, ignored by Dependabot) and `targetAbi` in `meta.json`, then build.

## License

[GPL-3.0](LICENSE), like most Jellyfin plugins.
