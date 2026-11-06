#!/usr/bin/env bash
# Génère dist/BetaSeries_<version>.zip et met à jour manifest.json (dépôt de plugins Jellyfin).
# Usage : ./release.sh <utilisateur/dépôt GitHub> [changelog]
#   ex. : ./release.sh timo/jellyfin-betaseries "Première version"
# Ensuite : créer une release GitHub avec le tag v<version> et y joindre le zip, puis commiter manifest.json.
set -euo pipefail
cd "$(dirname "$0")"
REPO=${1:?usage: ./release.sh <utilisateur/depot> [changelog]}
CHANGELOG=${2:-"Nouvelle version"}
./build.sh
VERSION=$(grep -o '"version": *"[^"]*"' meta.json | cut -d'"' -f4)
ABI=$(grep -o '"targetAbi": *"[^"]*"' meta.json | cut -d'"' -f4)
ZIP="dist/BetaSeries_${VERSION}.zip"
rm -f "$ZIP"
( cd "dist/BetaSeries_${VERSION}" && python3 -c "
import zipfile,glob,os
z=zipfile.ZipFile('../BetaSeries_${VERSION}.zip','w',zipfile.ZIP_DEFLATED)
[z.write(f) for f in sorted(os.listdir('.'))]
z.close()" )
CHECKSUM=$(md5sum "$ZIP" | cut -d' ' -f1)
URL="https://github.com/${REPO}/releases/download/v${VERSION}/BetaSeries_${VERSION}.zip"
python3 - "$VERSION" "$ABI" "$URL" "$CHECKSUM" "$CHANGELOG" <<'PY'
import json,sys,datetime,os
version,abi,url,checksum,changelog=sys.argv[1:6]
meta=json.load(open('meta.json'))
entry={"version":version,"changelog":changelog,"targetAbi":abi,"sourceUrl":url,"checksum":checksum,
       "timestamp":datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")}
manifest=json.load(open('manifest.json')) if os.path.exists('manifest.json') else []
plugin=next((p for p in manifest if p["guid"]==meta["guid"]),None)
if plugin is None:
    plugin={k:meta[k] for k in ("guid","name","description","overview","owner","category")}
    plugin["versions"]=[]
    manifest.append(plugin)
plugin["versions"]=[v for v in plugin["versions"] if v["version"]!=version]
plugin["versions"].insert(0,entry)
json.dump(manifest,open('manifest.json','w'),indent=2,ensure_ascii=False)
PY
echo "Zip : $ZIP (md5 $CHECKSUM)"
echo "URL du manifeste à ajouter dans Jellyfin : https://raw.githubusercontent.com/${REPO}/main/manifest.json"
