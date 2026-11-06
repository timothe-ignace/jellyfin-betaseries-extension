# Jellyfin BetaSeries

[![CI](https://github.com/timothe-ignace/jellyfin-betaseries-extension/actions/workflows/ci.yml/badge.svg)](https://github.com/timothe-ignace/jellyfin-betaseries-extension/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/timothe-ignace/jellyfin-betaseries-extension)](https://github.com/timothe-ignace/jellyfin-betaseries-extension/releases)
[![Licence](https://img.shields.io/github/license/timothe-ignace/jellyfin-betaseries-extension)](LICENSE)

Plugin [Jellyfin](https://jellyfin.org) qui marque automatiquement sur [BetaSeries](https://www.betaseries.com) les **épisodes et films que vous avez regardés**.

- **Multi-utilisateur** : chaque utilisateur Jellyfin lie son propre compte BetaSeries, depuis ses préférences.
- **Automatique** : dès qu'un épisode ou un film est terminé (ou marqué comme vu à la main), il est coché sur BetaSeries.
- **Sans stocker votre mot de passe** : seul le token BetaSeries est conservé.

> Compatible **Jellyfin 12.x**. Voir [Compatibilité](#compatibilité).

---

## Installation

### 1. Ajouter le dépôt de plugins

Dans Jellyfin, en tant qu'administrateur :

1. **Tableau de bord → Extensions → Dépôts → Ajouter**
2. Nom : `BetaSeries`
3. URL du dépôt :
   ```
   https://raw.githubusercontent.com/timothe-ignace/jellyfin-betaseries-extension/main/manifest.json
   ```

### 2. Installer le plugin

1. **Tableau de bord → Extensions → Catalogue**, cherchez **BetaSeries** et installez la dernière version.
2. **Redémarrez Jellyfin.**

### 3. Configurer la clé API (administrateur, une seule fois)

BetaSeries identifie les applications par une clé API.

1. Créez une application sur <https://www.betaseries.com/api/> et copiez son **Client ID**.
2. Dans Jellyfin : **Tableau de bord → Extensions → BetaSeries → Paramètres**, collez la clé, enregistrez.

Cette clé identifie seulement l'application : elle ne donne accès à aucun compte et est partagée par tous les utilisateurs.

### 4. Lier son compte (chaque utilisateur)

Ouvrez vos **préférences** Jellyfin (menu du profil) : une entrée **BetaSeries** y apparaît. Au choix :

- connectez-vous avec votre identifiant et mot de passe BetaSeries (utilisés une seule fois pour obtenir un token, jamais conservés), ou
- collez directement un token BetaSeries.

C'est tout : vos prochains visionnages sont synchronisés. Pour arrêter, cliquez sur **Délier mon compte** sur la même page.

> **Entrée absente du menu ?** L'ajout au menu des préférences passe par le plugin [JavaScript Injector](https://github.com/n00bcodr/Jellyfin-JavaScript-Injector) (à installer et à activer). Sans lui, la page reste accessible à l'adresse
> `https://<votre-jellyfin>/web/#/configurationpage?name=BetaSeriesUser`.

---

## Ce qui est synchronisé

| Élément | Condition | Identifiant utilisé |
|---|---|---|
| Épisode vu | terminé, ou marqué « vu » | ID **TVDB** de l'épisode |
| Film vu (option activable par l'admin) | terminé, ou marqué « vu » | ID **IMDb**, sinon **TMDb** |

- Seuls les **nouveaux** visionnages sont envoyés : l'historique existant n'est pas importé.
- Repasser un élément en « non vu » dans Jellyfin ne le décoche **pas** sur BetaSeries.
- Marquer un épisode n'en coche **pas** les précédents sur BetaSeries.
- Un élément sans identifiant TVDB/IMDb/TMDb (métadonnées manquantes) est ignoré ; une ligne le signale dans les logs.

## Dépannage

Cherchez `BetaSeries` dans les logs Jellyfin (*Tableau de bord → Journaux*, ou `docker logs jellyfin | grep BetaSeries`).

| Symptôme | Piste |
|---|---|
| « Le plugin n'est pas encore configuré » | L'administrateur n'a pas saisi la clé API (étape 3). |
| « Token enregistré mais invalide ou expiré » | Reconnectez-vous depuis votre page BetaSeries. |
| Rien ne se passe après un visionnage | Compte non lié, ou épisode sans ID TVDB : voir les logs. |
| Pas d'entrée BetaSeries dans les préférences | JavaScript Injector absent/désactivé, ou rechargez la page avec Ctrl+F5. |

## Sécurité et vie privée

- Les mots de passe BetaSeries ne sont jamais stockés ni journalisés.
- Le token de chaque utilisateur est enregistré **en clair** dans `config/plugins/configurations/Jellyfin.Plugin.BetaSeries.tokens.json` (droits `0600`). Protégez le dossier de configuration de Jellyfin comme le reste de votre installation.
- Chaque utilisateur ne peut lire et modifier que son propre token. La clé API n'est jamais renvoyée aux non-administrateurs.
- Les tokens des utilisateurs Jellyfin supprimés ne sont pas purgés automatiquement.
- Le seul service externe contacté est `api.betaseries.com`.

Une faille ? Ouvrez un [signalement privé](https://github.com/timothe-ignace/jellyfin-betaseries-extension/security/advisories/new).

## Compatibilité

| Plugin | Jellyfin |
|---|---|
| 1.0.x | 12.1+ (testé en 12.4) |

Le plugin est compilé pour une version précise de Jellyfin (`targetAbi`). Après une montée de version majeure de Jellyfin, attendez une nouvelle version du plugin.

---

## Contribuer / développer

Prérequis : Docker (le SDK .NET 10 est fourni par l'image officielle) ou un SDK .NET 10 local.

```bash
./build.sh                                   # compile → dist/BetaSeries_<version>/ et .zip
./build.sh ~/docker/jellyfin/config/plugins  # + copie dans le dossier plugins de Jellyfin
docker restart jellyfin
```

Structure :

```
Jellyfin.Plugin.BetaSeries/   code du plugin (C#) et pages web (Configuration/)
meta.json                     métadonnées du plugin (version, targetAbi…)
manifest.json                 manifeste du dépôt de plugins (généré par la CI)
scripts/                      empaquetage et mise à jour du manifeste
.github/workflows/            CI et release
```

### Workflow git

- Travaillez sur une branche (`feat/…`, `fix/…`) et ouvrez une pull request vers `main` ; la CI compile (warnings = erreurs) et empaquette.
- Messages de commit courts à l'impératif, en français ou en anglais.

### Publier une version

1. Dans une PR : incrémenter `version` dans `meta.json` **et** `AssemblyVersion`/`FileVersion` dans le `.csproj` (la CI vérifie qu'elles concordent) ; adapter `targetAbi` si besoin ; mettre à jour `CHANGELOG.md`.
2. Fusionner la PR, puis tagger `main` :
   ```bash
   git tag v1.1.0.0 && git push origin v1.1.0.0
   ```
3. La CI (`release.yml`) compile, crée la release GitHub avec le zip, puis commite `manifest.json` (URL + checksum MD5) sur `main`. Le plugin est alors proposé dans le catalogue Jellyfin.

`./release.sh <utilisateur/depot>` reproduit localement le zip et le manifeste (utile pour tester).

### Changer de version de Jellyfin

Ajuster `Jellyfin.Controller` dans le `.csproj` (volontairement figé, hors Dependabot) et `targetAbi` dans `meta.json`, puis compiler.

## Licence

[GPL-3.0](LICENSE), comme la plupart des plugins Jellyfin.
