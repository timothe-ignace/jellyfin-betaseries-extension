#!/usr/bin/env python3
"""Ajoute (ou remplace) une version dans manifest.json.

Usage : update-manifest.py <depot owner/name> <zip> <changelog>
Lit meta.json (ou $META_PATH) pour les métadonnées du plugin, calcule le MD5 du zip.
"""
import datetime
import hashlib
import json
import os
import sys

repo, zip_path, changelog = sys.argv[1:4]
meta = json.load(open(os.environ.get("META_PATH", "meta.json"), encoding="utf-8"))
version, abi = meta["version"], meta["targetAbi"]
assert version.endswith(".0"), "la version Jellyfin doit être X.Y.Z.0"
tag = "v" + version[:-2]
md5 = hashlib.md5(open(zip_path, "rb").read()).hexdigest()
entry = {
    "version": version,
    "changelog": changelog,
    "targetAbi": abi,
    "sourceUrl": f"https://github.com/{repo}/releases/download/{tag}/{os.path.basename(zip_path)}",
    "checksum": md5,
    "timestamp": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
}
manifest = json.load(open("manifest.json", encoding="utf-8")) if os.path.exists("manifest.json") else []
plugin = next((p for p in manifest if p["guid"] == meta["guid"]), None)
if plugin is None:
    plugin = {k: meta[k] for k in ("guid", "name", "description", "overview", "owner", "category")}
    plugin["versions"] = []
    manifest.append(plugin)
plugin["versions"] = [v for v in plugin["versions"] if v["version"] != version]
plugin["versions"].insert(0, entry)
with open("manifest.json", "w", encoding="utf-8") as f:
    json.dump(manifest, f, indent=2, ensure_ascii=False)
    f.write("\n")
print(f"manifest.json : {version} (md5 {md5})")
