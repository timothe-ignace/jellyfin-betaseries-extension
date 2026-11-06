#!/usr/bin/env bash
# Build local + zip + mise à jour de manifest.json (la CI fait la même chose sur un tag vX.Y.Z.W).
# Usage : ./release.sh <utilisateur/depot-github> [changelog]
set -euo pipefail
cd "$(dirname "$0")"
REPO=${1:?usage: ./release.sh <utilisateur/depot> [changelog]}
./build.sh
VERSION=$(python3 -c 'import json;print(json.load(open("meta.json"))["version"])')
scripts/update-manifest.py "$REPO" "dist/BetaSeries_${VERSION}.zip" "${2:-Nouvelle version}"
