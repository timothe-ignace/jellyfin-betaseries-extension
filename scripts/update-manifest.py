#!/usr/bin/env python3
"""Adds (or replaces) a version in manifest.json.

Usage: update-manifest.py <repo owner/name> <zip> <changelog>
Reads meta.json (or $META_PATH) for the plugin metadata and computes the zip MD5.
"""

import datetime
import hashlib
import json
import os
import sys

repo, zip_path, changelog = sys.argv[1:4]
with open(os.environ.get("META_PATH", "meta.json"), encoding="utf-8") as f:
    meta = json.load(f)
version, abi = meta["version"], meta["targetAbi"]
tag = "v" + version
with open(zip_path, "rb") as f:
    md5 = hashlib.md5(f.read()).hexdigest()
entry = {
    "version": version,
    "changelog": changelog,
    "targetAbi": abi,
    "sourceUrl": f"https://github.com/{repo}/releases/download/{tag}/{os.path.basename(zip_path)}",
    "checksum": md5,
    "timestamp": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
}
manifest = []
if os.path.exists("manifest.json"):
    with open("manifest.json", encoding="utf-8") as f:
        manifest = json.load(f)
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
print(f"manifest.json: {version} (md5 {md5})")
