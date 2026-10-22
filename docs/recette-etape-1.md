# Recette étape 1

## Tests automatisés fournis

- Création client + mission et réouverture de la même base.
- Isolation de la liste des missions par client.
- Rejet d’une référence déjà utilisée, y compris casse et espaces.
- Rejet des noms vides, e-mails invalides et clients inexistants.

## Recette manuelle Windows — à exécuter

1. Lancer dans une session Windows de test, sans base existante.
2. Créer « Entreprise fictive », sans e-mail.
3. Créer une mission TEST-001, intitulé Organisation, périmètre Flux documentaires.
4. Fermer et relancer : retrouver le client et la mission.
5. Créer un second client : sa liste de missions doit être vide.
6. Saisir TEST-001 à nouveau : vérifier le rejet et la conservation du formulaire.
7. Commencer une saisie puis fermer : choisir Non et vérifier que la saisie reste présente.
8. Vérifier clavier, redimensionnement et affichage à 100 % et 150 %.
9. Couper le réseau et recommencer les étapes 2 à 4.
10. Vérifier une erreur d’accès au dossier dans une session de test sans supprimer de données existantes.

Ces vérifications manuelles ne sont pas déclarées réussies par le simple passage de la CI.

## Résultats observés le 22 octobre 2026

SDK local 10.0.402 installé dans un environnement Linux. Compilation croisée de toute la solution, y compris WPF : réussite, 0 avertissement et 0 erreur. Quatre tests d’intégration SQLite exécutés : 4 réussis, 0 échec, 0 ignoré. Dépendance native SQLite mise à jour vers SQLitePCLRaw 2.1.14 après alerte NU1903 sur la version 2.1.11 ; restauration suivante sans cette alerte.

L’interface WPF n’a pas été exécutée dans cet environnement Linux. La recette manuelle Windows ci-dessus reste à faire.
