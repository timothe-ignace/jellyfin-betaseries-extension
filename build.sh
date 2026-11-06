#!/usr/bin/env bash
# Compile le plugin avec le SDK .NET 10 via Docker et prépare dist/BetaSeries_<version>/
# Usage : ./build.sh [dossier_plugins_jellyfin]   (ex. ./build.sh ~/docker/jellyfin/config/plugins)
set -euo pipefail
cd "$(dirname "$0")"
VERSION=$(python3 -c 'import json;print(json.load(open("meta.json"))["version"])')
OUT="dist/BetaSeries_${VERSION}"
rm -rf "$OUT" "build-out" && mkdir -p "$OUT"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp \
  -v "$PWD":/src -w /src/Jellyfin.Plugin.BetaSeries \
  mcr.microsoft.com/dotnet/sdk:10.0 dotnet build -c Release -o /src/build-out
scripts/package.sh build-out >/dev/null
rm -rf build-out
echo "Plugin prêt dans $OUT"
if [[ $# -ge 1 ]]; then
  mkdir -p "$1/BetaSeries_${VERSION}"
  cp "$OUT"/* "$1/BetaSeries_${VERSION}/"
  echo "Copié dans $1/BetaSeries_${VERSION} — redémarrez Jellyfin."
fi
