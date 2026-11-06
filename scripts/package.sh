#!/usr/bin/env bash
# Crée dist/BetaSeries_<version>.zip (DLL + meta.json) à partir d'un dossier de build.
# Usage : scripts/package.sh <dossier_de_build>
set -euo pipefail
cd "$(dirname "$0")/.."
BUILD=${1:?usage: scripts/package.sh <dossier_de_build>}
VERSION=$(python3 -c 'import json;print(json.load(open("meta.json"))["version"])')
STAGE="dist/BetaSeries_${VERSION}"
rm -rf "$STAGE" "dist/BetaSeries_${VERSION}.zip" && mkdir -p "$STAGE"
cp "$BUILD/Jellyfin.Plugin.BetaSeries.dll" meta.json "$STAGE/"
python3 - "$STAGE" "dist/BetaSeries_${VERSION}.zip" <<'PY'
import os, sys, zipfile
stage, out = sys.argv[1:3]
with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
    for f in sorted(os.listdir(stage)):
        z.write(os.path.join(stage, f), f)
PY
echo "dist/BetaSeries_${VERSION}.zip"
