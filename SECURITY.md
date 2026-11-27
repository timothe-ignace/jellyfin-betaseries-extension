# Security Policy

## Supported versions

Security fixes are released for the latest version only. Please check that the problem still happens with the [latest release](https://github.com/timothe-ignace/jellyfin-betaseries-extension/releases/latest) before reporting it.

## Reporting a vulnerability

**Please do not open a public issue, discussion or pull request for a security problem.**

Use GitHub's [private vulnerability reporting](https://github.com/timothe-ignace/jellyfin-betaseries-extension/security/advisories/new) instead.

Please include:

- the plugin and Jellyfin versions;
- the steps to reproduce the problem;
- what an attacker could do with it, and what access they need (for example: no account, a regular user or an administrator).

**Remove all secrets** (BetaSeries tokens, passwords, API keys, Jellyfin access tokens) from logs and screenshots.

This project is maintained on a best-effort basis. Please allow time for a fix to be released before disclosing the problem publicly.

## Scope

In scope: this plugin's code and its release packages. Examples:

- a user reading or changing another user's BetaSeries link;
- a non-administrator obtaining the API key;
- a password or token leaking, for example into the logs.

Vulnerabilities in [Jellyfin](https://github.com/jellyfin/jellyfin/security), the [JavaScript Injector](https://github.com/n00bcodr/Jellyfin-JavaScript-Injector) plugin or BetaSeries itself should be reported to their respective maintainers.

## Known limitation

BetaSeries tokens are stored unencrypted in the Jellyfin configuration folder, readable only by the account running Jellyfin on Linux and macOS (file mode `0600`). See [Security and privacy](README.md#security-and-privacy).
