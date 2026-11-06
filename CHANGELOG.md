# Changelog

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Changed
- Whole repository (code, UI, log and error messages, documentation, CI) translated to English.
- 4-part versioning (`X.Y.Z.W`) aligned with Jellyfin, tags `vX.Y.Z.W`.

## [1.0.1.0] - 2026-11-06

No functional change: sets up automated releases.

### Added
- GitHub Actions CI (build, packaging) and an automatic release workflow (zip, GitHub release, `manifest.json` update).
- Dependabot for GitHub Actions and NuGet packages.

### Changed
- README rewritten for end users.
- Newtonsoft.Json 13.0.4 (build-time dependency).

## [1.0.0.0] - 2026-11-06

### Added
- Sync of watched episodes (TVDB ID) and movies (IMDb/TMDb ID) to BetaSeries.
- Per-user account linking (login/password or token) from the profile settings.
- Admin page for the API key and the movie sync option.

### Security
- Validation of the token format and sign-in fields.
- Token file created directly with mode `0600`, atomic write.
