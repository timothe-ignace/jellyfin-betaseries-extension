# Changelog

Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/).

## [1.0.1] - 2026-11-06

Aucun changement fonctionnel : version de bascule vers le versionnage SemVer et la publication automatisée.

### Ajouté
- CI GitHub Actions (build, empaquetage) et workflow de release automatique sur tag `vX.Y.Z` (zip, release GitHub, mise à jour de `manifest.json`).
- Dependabot pour les actions GitHub et les paquets NuGet.

### Modifié
- Versionnage SemVer à 3 chiffres pour les tags (`vX.Y.Z`) ; la version Jellyfin reste `X.Y.Z.0`.
- README réécrit à destination des utilisateurs.
- Newtonsoft.Json 13.0.4 (dépendance de compilation).

## [1.0.0] - 2026-11-06

_Publiée sous le tag `v1.0.0.0`._

### Ajouté
- Synchronisation des épisodes (ID TVDB) et films (ID IMDb/TMDb) vus vers BetaSeries.
- Liaison de compte par utilisateur Jellyfin (identifiant/mot de passe ou token), depuis les préférences du profil.
- Page d'administration pour la clé API et l'option de synchronisation des films.

### Sécurité
- Validation du format du token et des champs de connexion.
- Fichier des tokens créé directement en droits `0600`, écriture atomique.
