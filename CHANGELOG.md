# Changelog

Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/).

## [Non publié]

### Modifié
- Versionnage à 4 chiffres (`X.Y.Z.W`) aligné sur Jellyfin, tags `vX.Y.Z.W` (le tag provisoire `v1.0.1` a été renommé `v1.0.1.0`).

## [1.0.1.0] - 2026-11-06

Aucun changement fonctionnel : mise en place de la publication automatisée.

### Ajouté
- CI GitHub Actions (build, empaquetage) et workflow de release automatique sur tag de version (zip, release GitHub, mise à jour de `manifest.json`).
- Dependabot pour les actions GitHub et les paquets NuGet.

### Modifié
- README réécrit à destination des utilisateurs.
- Newtonsoft.Json 13.0.4 (dépendance de compilation).

## [1.0.0.0] - 2026-11-06

_Publiée sous le tag `v1.0.0.0`._

### Ajouté
- Synchronisation des épisodes (ID TVDB) et films (ID IMDb/TMDb) vus vers BetaSeries.
- Liaison de compte par utilisateur Jellyfin (identifiant/mot de passe ou token), depuis les préférences du profil.
- Page d'administration pour la clé API et l'option de synchronisation des films.

### Sécurité
- Validation du format du token et des champs de connexion.
- Fichier des tokens créé directement en droits `0600`, écriture atomique.
