# Jellyfin BetaSeries

Plugin Jellyfin qui synchronise les épisodes et films **vus** vers [BetaSeries](https://www.betaseries.com), **par utilisateur** : chacun lie son propre compte depuis son profil.

Testé avec Jellyfin 12.4 (.NET 10).

## Fonctionnement

- L'administrateur renseigne la **clé API** d'une application BetaSeries (https://www.betaseries.com/api/) dans *Tableau de bord → Plugins → BetaSeries*.
- Chaque utilisateur ouvre l'entrée **BetaSeries** de son menu (page `BetaSeriesUser`) et :
  - se connecte avec son login / mot de passe BetaSeries (seul le token est conservé), ou
  - colle directement un token.
- Quand un épisode ou un film est terminé ou marqué comme vu, le plugin le marque comme vu sur BetaSeries :
  - épisodes : via l'ID **TVDB** de l'épisode (`POST /episodes/watched`, `bulk=false`) ;
  - films : via l'ID **IMDb** ou **TMDb** (`GET/POST /movies/movie`).
- Les éléments sans ID correspondant sont ignorés (voir les logs Jellyfin).
- Les tokens sont stockés dans `config/plugins/configurations/Jellyfin.Plugin.BetaSeries.tokens.json`, indexés par utilisateur Jellyfin.

## Compilation et installation

Prérequis : Docker (le SDK .NET 10 est tiré via l'image officielle).

```bash
./build.sh                                   # produit dist/BetaSeries_1.0.0.0/
./build.sh ~/docker/jellyfin/config/plugins  # + copie dans le dossier plugins de Jellyfin
docker restart jellyfin
```

Ou à la main : copier `Jellyfin.Plugin.BetaSeries.dll` et `meta.json` dans `<config>/plugins/BetaSeries_1.0.0.0/`, puis redémarrer Jellyfin.

## Dépôt de plugins (installation depuis Jellyfin)

Jellyfin installe les plugins depuis un manifeste JSON hébergé en ligne.

```bash
./release.sh <utilisateur/depot-github> "Notes de version"   # crée dist/BetaSeries_<v>.zip et manifest.json
```

1. Commiter `manifest.json` sur la branche `main`.
2. Créer une release GitHub au tag `v<version>` (ex. `v1.0.0.0`) et y joindre `dist/BetaSeries_<v>.zip`.
3. Dans Jellyfin : *Tableau de bord → Extensions → Dépôts → Ajouter*, URL :
   `https://raw.githubusercontent.com/<utilisateur>/<depot>/main/manifest.json`
4. Le plugin apparaît dans le catalogue, avec mises à jour automatiques.

Pour chaque nouvelle version : incrémenter `version` dans `meta.json` et `AssemblyVersion` dans le `.csproj`, relancer `release.sh`.

## Mise à jour de version de Jellyfin

Le paquet NuGet `Jellyfin.Controller` (`12.*`) définit l'ABI. Si Jellyfin change de version majeure, ajuster la version dans le `.csproj`, `targetAbi` dans `meta.json`, puis recompiler.

## Limites connues

- Seul le passage à « vu » est synchronisé (pas le « non vu »).
- Pas de synchro de l'historique existant : uniquement les nouveaux visionnages.
- L'API BetaSeries n'a pas été validée contre sa documentation officielle : vérifier les logs au premier test.

## Sécurité

- Les mots de passe BetaSeries ne sont jamais stockés ni journalisés : seul le token obtenu est conservé.
- Les tokens sont en clair dans `config/plugins/configurations/Jellyfin.Plugin.BetaSeries.tokens.json` (droits 0600). Quiconque a accès au dossier de config Jellyfin peut les lire : protégez-le comme le reste de la configuration.
- Chaque utilisateur ne peut lire/modifier que son propre token (ID tiré de l'authentification Jellyfin). La clé API n'est jamais renvoyée aux non-administrateurs.
- Les tokens des utilisateurs supprimés ne sont pas purgés automatiquement.

## Licence

GPL-3.0, comme la plupart des plugins Jellyfin (voir `LICENSE`).
