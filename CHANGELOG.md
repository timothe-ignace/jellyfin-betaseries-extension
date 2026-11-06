# Changelog

Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/).

## [Non publié]

### Ajouté
- CI GitHub Actions (build, empaquetage) et workflow de release automatique (zip, release GitHub, mise à jour de `manifest.json`).
- Dependabot pour les actions GitHub et les paquets NuGet.

### Modifié
- README réécrit à destination des utilisateurs.

## [1.0.0.0] - 2026-11-06

### Ajouté
- Synchronisation des épisodes (ID TVDB) et films (ID IMDb/TMDb) vus vers BetaSeries.
- Liaison de compte par utilisateur Jellyfin (identifiant/mot de passe ou token), depuis les préférences du profil.
- Page d'administration pour la clé API et l'option de synchronisation des films.

### Sécurité
- Validation du format du token et des champs de connexion.
- Fichier des tokens créé directement en droits `0600`, écriture atomique.
