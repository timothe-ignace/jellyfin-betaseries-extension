#!/usr/bin/env bash
# Local build + zip + manifest.json update (CI does the same on a vX.Y.Z.W tag).
# Usage: ./release.sh <github-user/repo> [changelog]
set -euo pipefail
cd "$(dirname "$0")"
REPO=${1:?usage: ./release.sh <user/repo> [changelog]}
./build.sh
VERSION=$(python3 -c 'import json;print(json.load(open("meta.json"))["version"])')
scripts/update-manifest.py "$REPO" "dist/BetaSeries_${VERSION}.zip" "${2:-New version}"
